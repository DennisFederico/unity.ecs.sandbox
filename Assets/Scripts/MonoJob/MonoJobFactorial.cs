using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MonoJob {
   
    public class MonoJobFactorial : MonoBehaviour {
        private void Update() {
            NativeReference<int> data1 = new NativeReference<int>(5, Allocator.TempJob);
            var jh1 = new FactorialJob() {
                n = data1
            }.Schedule();
            jh1 = new FactorialJob() {
                n = data1
            }.Schedule(jh1);
            
            NativeReference<int> data2 = new NativeReference<int>(5, Allocator.TempJob);
            var jh2 = new FactorialJob() {
                n = data2
            }.Schedule();
            jh2 = new FactorialJob() {
                n = data2
            }.Schedule(jh2);
      
            NativeReference<int> data3 = new NativeReference<int>(5, Allocator.TempJob);
            var jh3 = new FactorialJob() {
                n = data3
            }.Schedule();
            jh3 = new FactorialJob() {
                n = data3
            }.Schedule(jh3);

            var allJh = JobHandle.CombineDependencies(jh1, jh2, jh3);
            StartCoroutine(LogAndDisposeJob(allJh, new[] {data1, data2, data3}));
        }
        
        private IEnumerator LogAndDisposeJob(JobHandle jobHandle, NativeReference<int>[] data) {
            if (!jobHandle.IsCompleted) yield return null;
            //yield return new WaitUntil(() => jobHandle.IsCompleted);
            jobHandle.Complete();
            foreach (var reference in data) {
                Debug.Log($"Results = {reference.Value}");
                reference.Dispose();
            }
        }
    }
    


    public struct FactorialJob : IJob {

        public NativeReference<int> n;
        
        public void Execute() {
            var result = 1;
            for (int i = 1; i < n.Value; i++) {
                result *= i;
                Debug.Log($"Factorial {i}! = {result}");
            }
            n.Value = result;
        }
    }
}