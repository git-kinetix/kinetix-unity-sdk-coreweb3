using System;
using System.Threading.Tasks;
using Kinetix.Internal;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace Kinetix.Tests
{
    public class OperationManagerMultiTests : MonoBehaviour
    {
        [Test]
        public async Task SimilarConfigSync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA       opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = idOpA;
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA       opB       = new OperationTestA(opConfigB);

            Task<OperationTestResponse> taskA = operationManager.RequestExecution(opA);
            Task<OperationTestResponse> taskB = operationManager.RequestExecution(opB);
            await Task.WhenAll(taskA, taskB);
            
            if (taskA.Result.operationId != idOpA)
                Assert.Fail();
            if (taskB.Result.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        [Test]
        public async Task SimilarConfigAsync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA       opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = idOpA;
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA       opB       = new OperationTestA(opConfigB);

            OperationTestResponse responseA = await operationManager.RequestExecution(opA);
            OperationTestResponse responseB = await operationManager.RequestExecution(opB);

            if (responseA.operationId != idOpA)
            {
                Assert.Fail();
            }

            
            if (responseB.operationId != idOpB)
            {
               Assert.Fail();
            }
            
            
            Assert.Pass();
        }
        
        [Test]
        public async Task DifferentConfigSync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA       opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA       opB       = new OperationTestA(opConfigB);

            Task<OperationTestResponse> taskA = operationManager.RequestExecution(opA);
            Task<OperationTestResponse> taskB = operationManager.RequestExecution(opB);
            await Task.WhenAll(taskA, taskB);
            
            if (taskA.Result.operationId != idOpA)
                Assert.Fail();
            if (taskB.Result.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        [Test]
        public async Task DifferentConfigAsync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA       opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA       opB       = new OperationTestA(opConfigB);

            OperationTestResponse responseA = await operationManager.RequestExecution(opA);
            OperationTestResponse responseB = await operationManager.RequestExecution(opB);
            
            if (responseA.operationId != idOpA)
                Assert.Fail();
            if (responseB.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        [Test]
        public async Task DifferentOperationsSync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA      opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestB      opB       = new OperationTestB(opConfigB);

            Task<OperationTestResponse> taskA = operationManager.RequestExecution(opA);
            Task<OperationTestResponse> taskB = operationManager.RequestExecution(opB);
            await Task.WhenAll(taskA, taskB);
            
            if (taskA.Result.operationId != idOpA)
                Assert.Fail();
            if (taskB.Result.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        [Test]
        public async Task DifferentOperationsAsync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA      opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestB      opB       = new OperationTestB(opConfigB);

            OperationTestResponse responseA = await operationManager.RequestExecution(opA);
            OperationTestResponse responseB = await operationManager.RequestExecution(opB);
            
            if (responseA.operationId != idOpA)
                Assert.Fail();
            if (responseB.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        
        [Test]
        public async Task SimilarDelayedConfigAsync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA      opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = idOpA;
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA      opB       = new OperationTestA(opConfigB);

            OperationTestResponse responseA = await operationManager.RequestExecution(opA);
            await Task.Delay(500);
            OperationTestResponse responseB = await operationManager.RequestExecution(opB);
            
            if (responseA.operationId != idOpA)
                Assert.Fail();
            if (responseB.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
        
        [Test]
        public async Task DifferentDelayedConfigAsync()
        {
            OperationManager operationManager = new OperationManager();

            string              idOpA     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigA = new OperationTestConfig(idOpA);
            OperationTestA      opA       = new OperationTestA(opConfigA);
            
            string              idOpB     = Guid.NewGuid().ToString();
            OperationTestConfig opConfigB = new OperationTestConfig(idOpB);
            OperationTestA      opB       = new OperationTestA(opConfigB);

            OperationTestResponse responseA = await operationManager.RequestExecution(opA);
            await Task.Delay(500);
            OperationTestResponse responseB = await operationManager.RequestExecution(opB);
            
            if (responseA.operationId != idOpA)
                Assert.Fail();
            if (responseB.operationId != idOpB)
                Assert.Fail();
            Assert.Pass();
        }
    }
}
