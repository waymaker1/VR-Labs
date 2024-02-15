﻿using Zinnia.Cast;
using Zinnia.Data.Collection.List;
using Zinnia.Rule;

namespace Test.Zinnia.Cast
{
    using NUnit.Framework;
    using System.Collections;
    using Test.Zinnia.Utility.Mock;
    using Test.Zinnia.Utility.Stub;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.TestTools.Utils;

    public class ParabolicLineCastTest
    {
        private GameObject containingObject;
        private ParabolicLineCast subject;
        private GameObject validSurface;

        [SetUp]
        public void SetUp()
        {
#if UNITY_2022_2_OR_NEWER
            Physics.simulationMode = SimulationMode.Script;
#else
            Physics.autoSimulation = false;
#endif
            containingObject = new GameObject("ParabolicLineCastTest");
            subject = containingObject.AddComponent<ParabolicLineCast>();
            validSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(containingObject);
            Object.DestroyImmediate(validSurface);
#if UNITY_2022_2_OR_NEWER
            Physics.simulationMode = SimulationMode.FixedUpdate;
#else
            Physics.autoSimulation = true;
#endif
        }

        [Test]
        public void CastPointsValidTarget()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, -0.124f, 2.89f),
                new Vector3(0f, -1.4f, 4.4f),
                new Vector3(0f, -2.8f, 4.9f),
                new Vector3(0f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.AreEqual(validSurface.transform, subject.TargetHit.Value.transform);
            Assert.IsTrue(subject.IsTargetHitValid);
            Assert.IsTrue(castResultsChangedMock.Received);
        }

        [Test]
        public void CastPointsInsufficientForwardBeamLength()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(2f, 5f);
            subject.SegmentCount = 5;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, 0.4f, 1.2f),
                new Vector3(0f, 0.4f, 1.7f),
                new Vector3(0f, 0.14f, 1.96f),
                new Vector3(0f, 0.0001f, 1.9f)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.IsFalse(subject.TargetHit.HasValue);
            Assert.IsTrue(castResultsChangedMock.Received);
        }

        [Test]
        public void CastPointsInsufficientDownwardBeamLength()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 2f);
            subject.SegmentCount = 5;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, 0.4f, 2.9f),
                new Vector3(0f, 0.4f, 4.4f),
                new Vector3(0f, 0.14f, 4.9f),
                new Vector3(0f, 0.0001f, 4.9f)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.IsFalse(subject.TargetHit.HasValue);
            Assert.IsTrue(castResultsChangedMock.Received);
        }

        [UnityTest]
        public IEnumerator CastPointsInvalidTarget()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;
            validSurface.AddComponent<RuleStub>();
            NegationRule negationRule = validSurface.AddComponent<NegationRule>();
            AnyComponentTypeRule anyComponentTypeRule = validSurface.AddComponent<AnyComponentTypeRule>();
            SerializableTypeComponentObservableList rules = containingObject.AddComponent<SerializableTypeComponentObservableList>();
            anyComponentTypeRule.ComponentTypes = rules;
            rules.Add(typeof(RuleStub));
            yield return null;

            negationRule.Rule = new RuleContainer
            {
                Interface = anyComponentTypeRule
            };
            subject.TargetValidity = new RuleContainer
            {
                Interface = negationRule
            };

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, -0.12f, 2.89f),
                new Vector3(0f, -1.4f, 4.4f),
                new Vector3(0f, -2.8f, 4.9f),
                new Vector3(0f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.AreEqual(validSurface.transform, subject.TargetHit.Value.transform);
            Assert.IsFalse(subject.IsTargetHitValid);
            Assert.IsTrue(castResultsChangedMock.Received);
        }

        [Test]
        public void EventsNotEmittedOnInactiveGameObject()
        {
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;
            subject.gameObject.SetActive(false);

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Assert.AreEqual(0, subject.Points.Count);
            Assert.IsFalse(subject.TargetHit.HasValue);
            Assert.IsFalse(castResultsChangedMock.Received);
        }

        [Test]
        public void EventsNotEmittedOnDisabledComponent()
        {
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;
            subject.enabled = false;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Assert.AreEqual(0, subject.Points.Count);
            Assert.IsFalse(subject.TargetHit.HasValue);
            Assert.IsFalse(castResultsChangedMock.Received);
        }

        [Test]
        public void CursorLockDuration()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            UnityEventListenerMock castResultsChangedMock = new UnityEventListenerMock();
            subject.ResultsChanged.AddListener(castResultsChangedMock.Listen);
            subject.Origin = subject.gameObject;
            subject.CursorLockThreshold = 0.2f;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, -0.124f, 2.89f),
                new Vector3(0f, -1.4f, 4.4f),
                new Vector3(0f, -2.8f, 4.9f),
                new Vector3(0f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.AreEqual(validSurface.transform, subject.TargetHit.Value.transform);
            Assert.IsTrue(subject.IsTargetHitValid);
            Assert.IsTrue(castResultsChangedMock.Received);

            castResultsChangedMock.Reset();

            subject.gameObject.transform.position = Vector3.right * 0.15f;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            expectedPoints = new Vector3[]
            {
                Vector3.right * 0.15f,
                new Vector3(0.126f, -0.124f, 2.89f),
                new Vector3(0.075f, -1.4f, 4.4f),
                new Vector3(0.023f, -2.8f, 4.9f),
                new Vector3(0f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.AreEqual(validSurface.transform, subject.TargetHit.Value.transform);
            Assert.IsTrue(subject.IsTargetHitValid);
            Assert.IsTrue(castResultsChangedMock.Received);

            castResultsChangedMock.Reset();

            subject.gameObject.transform.position = Vector3.right * 0.25f;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();

            expectedPoints = new Vector3[]
            {
                Vector3.right * 0.25f,
                new Vector3(0.25f, -0.124f, 2.89f),
                new Vector3(0.25f, -1.4f, 4.4f),
                new Vector3(0.25f, -2.8f, 4.9f),
                new Vector3(0.25f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            Assert.AreEqual(validSurface.transform, subject.TargetHit.Value.transform);
            Assert.IsTrue(subject.IsTargetHitValid);
            Assert.IsTrue(castResultsChangedMock.Received);
        }

        [UnityTest]
        public IEnumerator TransitionDuration()
        {
            Vector3EqualityComparer comparer = new Vector3EqualityComparer(0.1f);
            subject.Origin = subject.gameObject;
            subject.TransitionDuration = 1f;

            validSurface.transform.position = Vector3.forward * 5f + Vector3.down * 4f;

            subject.MaximumLength = new Vector2(5f, 5f);
            subject.SegmentCount = 5;

            yield return null;

            Physics.Simulate(Time.fixedDeltaTime);
            subject.Process();
            yield return null;

            Vector3[] expectedPoints = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, -0.124f, 2.89f),
                new Vector3(0f, -1.4f, 4.4f),
                new Vector3(0f, -2.8f, 4.9f),
                new Vector3(0f, validSurface.transform.position.y + (validSurface.transform.localScale.y / 2f), validSurface.transform.position.z)
            };

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index);
            }

            subject.gameObject.transform.eulerAngles = Vector3.up * 15f;

            float timePassed = 0f;
            while (timePassed < subject.TransitionDuration)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                subject.Process();
                yield return new WaitForEndOfFrame();
                timePassed += Time.deltaTime;
            }

            expectedPoints = new Vector3[]
{
                Vector3.zero,
                new Vector3(0.450f, 0.421f, 2.855f),
                new Vector3(0.679f, 0.375f, 4.321f),
                new Vector3(0.768f, 0.140f, 4.86f),
                new Vector3(0.779f, 0.0001f, 4.938f)
};

            for (int index = 0; index < subject.Points.Count; index++)
            {
                Assert.That(subject.Points[index], Is.EqualTo(expectedPoints[index]).Using(comparer), "Index " + index + " = " + subject.Points[index].x + ", " + subject.Points[index].y + ", " + subject.Points[index].z);
            }
        }
    }
}