using Selection.Components;
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
        private EntityQuery _selectedPlayersQuery;
        private EntityQuery _unSelectedPlayersQuery;

        //NOTE. Alternatively we can use LookUp<T>. Don't really know what's the difference between the two at this point
        //TODO. Compare the performance of the two
        private struct Handles {
            public ComponentTypeHandle<PlayerNameComponent> PlayerNameType;
            public ComponentTypeHandle<IsSelectedComponent> IsSelectedType;

            public Handles(ref SystemState state) {
                PlayerNameType = state.GetComponentTypeHandle<PlayerNameComponent>();
                IsSelectedType = state.GetComponentTypeHandle<IsSelectedComponent>();
            }

            public void Update(ref SystemState state) {
                PlayerNameType.Update(ref state);
                IsSelectedType.Update(ref state);
            }
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _handles = new Handles(ref state);

            _selectedPlayersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerNameComponent>()
                .WithAll<IsSelectedComponent>()
                .Build(ref state);

            _unSelectedPlayersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerNameComponent>()
                .WithNone<IsSelectedComponent>()
                .Build(ref state);

            state.RequireForUpdate<PrefabHolderComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            BufferFromEntity<Child> childBuffer =
            
            // Creating entities from an Archetype is faster than adding components to an entity one by one since it avoids structural changes to the entity manager.
            NativeArray<ComponentType> components = new NativeArray<ComponentType>(3, Allocator.Temp) {
                [0] = ComponentType.ReadOnly<PlayerNameComponent>(),
                [1] = ComponentType.ReadWrite<TeamMemberComponent>(),
                [2] = ComponentType.ReadWrite<IsSelectedComponent>(),
            };
            var entityArchetype = state.EntityManager.CreateArchetype(components);
            components.Dispose();

            // Active entities
            for (int i = 0; i < 5; ++i) {
                Entity entity = state.EntityManager.CreateEntity(entityArchetype);
                state.EntityManager.SetComponentData(entity, new PlayerNameComponent($"Bench {i + 1} Blue"));
                state.EntityManager.SetComponentData(entity, new TeamMemberComponent() { Team = Team.Blue });
                state.EntityManager.SetComponentEnabled<IsSelectedComponent>(entity, true); //Optional - default is true
            }

            // Inactive entities
            for (int i = 5; i < 8; ++i) {
                Entity entity = state.EntityManager.CreateEntity(entityArchetype);
                state.EntityManager.SetComponentData(entity, new PlayerNameComponent($"Bench {i + 1} Red"));
                state.EntityManager.SetComponentData(entity, new TeamMemberComponent() { Team = Team.Red });
                state.EntityManager.SetComponentEnabled<IsSelectedComponent>(entity, false);
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
            PrintActivePlayerNames printJob = new() {
                PlayerNameType = _handles.PlayerNameType,
            };
            state.Dependency = printJob.Schedule(_selectedPlayersQuery, state.Dependency);

            // Print enabled entities using IJobEntity
            // TODO. Compare the performance of the two
            state.Dependency = new PrintPlayerNames() { Prefix = "On Bench:" }.Schedule(_unSelectedPlayersQuery, state.Dependency);

            state.Dependency = new AddSelectedVisualJobEntity() {
                ECB = ecb,
                VisualPrefab = visualPrefab.Prefab
            }.Schedule(_selectedPlayersQuery, state.Dependency);
            
            state.Dependency = new RemoveSelectedVisualJobEntity() {
                ECB = ecb,
            }.Schedule(_unSelectedPlayersQuery, state.Dependency);
            
            // Toggle enabled players
            TogglePlayerSelectedJob toggleJob = new() {
                IsSelectedType = _handles.IsSelectedType,
            };
            state.Dependency = toggleJob.Schedule(_selectedPlayersQuery, state.Dependency);
            
            //TODO. Updated visuals, filter Bench, etc.
            //TODO. Exercise: Add a new system that prints the names of the disabled players - Using a IJobEntity and adding withNone to the _selectedPlayersQuery
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        private struct TogglePlayerSelectedJob : IJobChunk {
            public ComponentTypeHandle<IsSelectedComponent> IsSelectedType;

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
                    Debug.Log($"Playing: {players[i].PlayerNameValue.Value}");
                }
            }
        }

        [BurstCompile]
        private partial struct PrintPlayerNames : IJobEntity {
            public FixedString32Bytes Prefix;

            private void Execute(in PlayerNameComponent playerNameComponent) {
                Debug.Log($"{Prefix.Value} {playerNameComponent.PlayerNameValue.Value}");
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
            public BufferLookup<Child> ChildBuffer;
            private void Execute(Entity entity) {
                
                // DynamicBuffer<Child> dynamicBuffer = SystemAPI.GetBuffer<Child>(entity);
                // Debug.Log($"Entity: {entity} has {childBuffer.Length} children");
                //var childBuffer = SystemAPI.GetBuffer<Child>(entity);
                //foreach (var child in childBuffer) {
                //if (SystemAPI.HasComponent<DecalComponentTag>(child.Value)) {
                //ecb.DestroyEntity(child.Value);
                //}
                //}
            }
        }
    }
}