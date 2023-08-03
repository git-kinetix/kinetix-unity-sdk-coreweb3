using System;
using System.Threading.Tasks;
using Kinetix.Internal;
using NUnit.Framework;
using UnityEngine;

namespace Kinetix.Tests
{
    public class OperationManagerStandardTests
    {
        [Test]
        public async Task OperationValid()
        {
            OperationManager operationManager = new OperationManager();

            OperationTestConfig opConfig = new OperationTestConfig(System.Guid.NewGuid().ToString());
            OperationTestA       op       = new OperationTestA(opConfig);

            try
            {
                OperationTestResponse opResponse = await operationManager.RequestExecution(op);
                Assert.IsTrue(op.Config.RandomId == opResponse.operationId);
            }
            catch (TaskCanceledException e)
            {
                Assert.Fail(e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public async Task OperationWithCancel()
        {
            OperationManager operationManager = new OperationManager();

            OperationTestConfig opConfig = new OperationTestConfig(System.Guid.NewGuid().ToString())
            {
                willCancel = true
            };

            OperationTestA op = new OperationTestA(opConfig);

            try
            {
                await operationManager.RequestExecution(op);
                Assert.Fail();
            }
            catch (TaskCanceledException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Assert.Fail();
            }
        }

        [Test]
        public async Task OperationWithException()
        {
            OperationManager operationManager = new OperationManager();

            OperationTestConfig opConfig = new OperationTestConfig(System.Guid.NewGuid().ToString())
            {
                willThrow = true
            };

            OperationTestA op = new OperationTestA(opConfig);

            try
            {
                await operationManager.RequestExecution(op);
                Assert.Fail();
            }
            catch (TaskCanceledException)
            {
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }
    }
}
