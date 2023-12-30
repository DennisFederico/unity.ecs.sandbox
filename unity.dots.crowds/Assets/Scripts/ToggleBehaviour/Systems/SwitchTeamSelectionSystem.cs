using ToggleBehaviour.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ToggleBehaviour.Systems {
    public partial struct SwitchTeamSelectionSystem : ISystem {
        private Handles _handles;
        private EntityQuery _selectedActivePlayersQuery;
        private EntityQuery _unSelectedActivePlayersQuery;
        private EntityQuery _benchedPlayersQuery;
        private BufferLookup<Child> _childBuffer;
        private ComponentLookup<VisualComponentTag> _visualComponentLookup;

        //NOTE. Alternatively we can use LookUp<T>. Don't really know what's the difference between the two at this point
        //TODO. Compare the performance of the two
        private struct Handles {
            public ComponentTypeHandle<PlayerNameComponent> PlayerNameType;
            public ComponentTypeHandle<IsSelectedComponentTag> IsSelectedType;

            public Handles(ref SystemState state) {
                PlayerNameType = state.GetComponentTypeHandle<PlayerNameComponent>();
                IsSelectedType = state.GetComponentTypeHandle<IsSelectedComponentTag>();
            }

            public void Update(ref SystemState state) {
                PlayerNameType.Update(ref state);
                IsSelectedType.Update(ref state);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _handles = new Handles(ref state);

            _selectedActivePlayersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerNameComponent>()
                .WithAll<IsSelectedComponentTag>()
                .WithAll<IsPlayingComponentTag>()
                .Build(ref state);

            _unSelectedActivePlayersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerNameComponent>()
                .WithNone<IsSelectedComponentTag>()
                .WithAll<IsPlayingComponentTag>()
                .Build(ref state);
            
            _benchedPlayersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerNameComponent>()
                .WithNone<IsPlayingComponentTag>()
                .Build(ref state);
            

            state.RequireForUpdate<PrefabHolderComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

            _childBuffer = SystemAPI.GetBufferLookup<Child>(true);
            _visualComponentLookup = SystemAPI.GetComponentLookup<VisualComponentTag>(true);

            // Creating entities from an Archetype is faster than adding components to an entity one by one since it avoids structural changes to the entity manager.
            NativeArray<ComponentType> components = new NativeArray<ComponentType>(4, Allocator.Temp) {
                [0] = ComponentType.ReadOnly<PlayerNameComponent>(),
                [1] = ComponentType.ReadWrite<TeamMemberComponent>(),
                [2] = ComponentType.ReadWrite<IsSelectedComponentTag>(),
                [3] = ComponentType.ReadWrite<IsPlayingComponentTag>()
            };
            var entityArchetype = state.EntityManager.CreateArchetype(components);
            components.Dispose();

            // Active Entities are Authored from the scene
            // Inactive (benched) Entities - Blue Team (Selected by default)
            for (int i = 0; i < 5; ++i) {
                Entity entity = state.EntityManager.CreateEntity(entityArchetype);
                state.EntityManager.SetComponentData(entity, new PlayerNameComponent($"Bench {i + 1} Blue"));
                state.EntityManager.SetComponentData(entity, new TeamMemberComponent() { Team = Team.Blue });
                state.EntityManager.SetComponentEnabled<IsSelectedComponentTag>(entity, true); //Optional - default is true
                state.EntityManager.SetComponentEnabled<IsPlayingComponentTag>(entity, false);
            }

            // Inactive (benched) entities - Red Team
            for (int i = 5; i < 8; ++i) {
                Entity entity = state.EntityManager.CreateEntity(entityArchetype);
                state.EntityManager.SetComponentData(entity, new PlayerNameComponent($"Bench {i + 1} Red"));
                state.EntityManager.SetComponentData(entity, new TeamMemberComponent() { Team = Team.Red });
                state.EntityManager.SetComponentEnabled<IsSelectedComponentTag>(entity, false);
                state.EntityManager.SetComponentEnabled<IsPlayingComponentTag>(entity, false);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var visualPrefab = SystemAPI.GetSingleton<PrefabHolderComponent>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            _handles.Update(ref state);

            // Print enabled entities
            // PrintActivePlayerNames printJob = new() {
            //     PlayerNameType = _handles.PlayerNameType,
            // };
            // state.Dependency = printJob.Schedule(_selectedActivePlayersQuery, state.Dependency);

            // Print enabled entities using IJobEntity
            // TODO. Compare the performance of the two
            state.Dependency = new PrintPlayerNames() { Prefix = "Selected:" }.Schedule(_selectedActivePlayersQuery, state.Dependency);
            state.Dependency = new PrintPlayerNames() { Prefix = "UnSelected:" }.Schedule(_unSelectedActivePlayersQuery, state.Dependency);
            state.Dependency = new PrintPlayerNames() { Prefix = "Benched:" }.Schedule(_benchedPlayersQuery, state.Dependency);

            state.Dependency = new AddSelectedVisualJobEntity() {
                ECB = ecb,
                VisualPrefab = visualPrefab.Prefab
            }.Schedule(_selectedActivePlayersQuery, state.Dependency);

            _childBuffer.Update(ref state);
            _visualComponentLookup.Update(ref state);
            state.Dependency = new RemoveSelectedVisualJobEntity() {
                ECB = ecb,
                ChildBuffer = _childBuffer,
                VisualComponentLookup = _visualComponentLookup
            }.Schedule(_unSelectedActivePlayersQuery, state.Dependency);

            // Toggle enabled players
            TogglePlayerSelectedJob toggleJob = new() {
                IsSelectedType = _handles.IsSelectedType,
            };
            state.Dependency = toggleJob.Schedule(_selectedActivePlayersQuery, state.Dependency);

            //TODO. Filter players on the bench (don't add selected visuals)
            //TODO. Handle Selected team from a World State
            //TODO. Bench Players (remind to handle the visual) -> BenchPlayers can become other model or disable the render component??
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        private struct TogglePlayerSelectedJob : IJobChunk {
            public ComponentTypeHandle<IsSelectedComponentTag> IsSelectedType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                //NOTE. we can access all data in the chunk even the ones with disabled components. If we want to skip disabled components, we have to use ChunkEntityEnumerator
                for (int i = 0; i < chunk.Count; i++) {
                    bool isActive = chunk.IsComponentEnabled(ref this.IsSelectedType, i);
                    chunk.SetComponentEnabled(ref this.IsSelectedType, i, !isActive);
                }
            }
        }

        [BurstCompile]
        private struct PrintActivePlayerNames : IJobChunk {
            [ReadOnly] public ComponentTypeHandle<PlayerNameComponent> PlayerNameType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
                NativeArray<PlayerNameComponent> players = chunk.GetNativeArray(ref PlayerNameType);
                ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (enumerator.NextEntityIndex(out int i)) {
                    Debug.Log($"Playing: {players[i].PlayerNameValue}");
                }
            }
        }

        [BurstCompile]
        private partial struct PrintPlayerNames : IJobEntity {
            public FixedString32Bytes Prefix;

            private void Execute(in PlayerNameComponent playerNameComponent) {
                Debug.Log($"{Prefix.Value} {playerNameComponent.PlayerNameValue}");
            }
        }

        [BurstCompile]
        private partial struct AddSelectedVisualJobEntity : IJobEntity {
            public EntityCommandBuffer ECB;
            [ReadOnly] public Entity VisualPrefab;

            private void Execute(Entity entity) {
                var selectedVisual = ECB.Instantiate(VisualPrefab);
                ECB.AddComponent(selectedVisual, new Parent() {
                    Value = entity
                });
            }
        }

        [BurstCompile]
        private partial struct RemoveSelectedVisualJobEntity : IJobEntity {
            public EntityCommandBuffer ECB;
            [ReadOnly] public BufferLookup<Child> ChildBuffer;
            [ReadOnly] public ComponentLookup<VisualComponentTag> VisualComponentLookup;

            private void Execute(Entity entity) {
                if (!ChildBuffer.HasBuffer(entity) || ChildBuffer[entity].Length <= 0) return;
                var children = ChildBuffer[entity];
                foreach (var child in children) {
                    if (VisualComponentLookup.HasComponent(child.Value)) {
                        ECB.DestroyEntity(child.Value);
                    }
                }
            }
        }
    }
}