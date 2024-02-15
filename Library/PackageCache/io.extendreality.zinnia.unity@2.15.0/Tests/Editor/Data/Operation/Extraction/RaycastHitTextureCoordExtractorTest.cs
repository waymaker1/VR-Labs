﻿using Zinnia.Data.Operation.Extraction;

namespace Test.Zinnia.Data.Operation.Extraction
{
    using NUnit.Framework;
    using System.Collections;
    using Test.Zinnia.Utility.Helper;
    using Test.Zinnia.Utility.Mock;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.TestTools.Utils;

    public class RaycastHitTextureCoordExtractorTest
    {
        private GameObject containingObject;
        private RaycastHitTextureCoordExtractor subject;

        [SetUp]
        public void SetUp()
        {
            containingObject = new GameObject("RaycastHitTextureCoordExtractorTest");
            subject = containingObject.AddComponent<RaycastHitTextureCoordExtractor>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(containingObject);
        }

        [Test]
        public void Extract()
        {
            Vector2EqualityComparer comparer = new Vector2EqualityComparer(0.1f);
            UnityEventListenerMock extractedMock = new UnityEventListenerMock();
            subject.Extracted.AddListener(extractedMock.Listen);
            RaycastHit hitData = RaycastHitHelper.GetRaycastHit();
            subject.Source = hitData;

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);

            subject.Extract();

            Assert.IsTrue(extractedMock.Received);
            Assert.That(subject.Result, Is.EqualTo(Vector2.zero).Using(comparer));
        }

        [Test]
        public void ExtractInvalidSource()
        {
            UnityEventListenerMock extractedMock = new UnityEventListenerMock();
            subject.Extracted.AddListener(extractedMock.Listen);

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);

            subject.Extract();

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);
        }

        [UnityTest]
        public IEnumerator ExtractNoCollisionData()
        {
            UnityEventListenerMock extractedMock = new UnityEventListenerMock();
            subject.Extracted.AddListener(extractedMock.Listen);

            yield return null;

            GameObject blocker = RaycastHitHelper.CreateBlocker("RaycastHitTextureCoordExtractorTest");
            blocker.SetActive(false);
            RaycastHit hitData = RaycastHitHelper.GetRaycastHit(blocker);

            subject.Source = hitData;

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);

            subject.Extract();

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);
        }

        [Test]
        public void ExtractInactiveGameObject()
        {
            UnityEventListenerMock extractedMock = new UnityEventListenerMock();
            subject.Extracted.AddListener(extractedMock.Listen);
            RaycastHit hitData = RaycastHitHelper.GetRaycastHit();
            subject.Source = hitData;
            subject.gameObject.SetActive(false);

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);

            subject.Extract();

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);
        }

        [Test]
        public void ExtractInactiveComponent()
        {
            UnityEventListenerMock extractedMock = new UnityEventListenerMock();
            subject.Extracted.AddListener(extractedMock.Listen);
            RaycastHit hitData = RaycastHitHelper.GetRaycastHit();

            subject.Source = hitData;
            subject.enabled = false;

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);

            subject.Extract();

            Assert.IsFalse(extractedMock.Received);
            Assert.IsFalse(subject.Result.HasValue);
        }
    }
}