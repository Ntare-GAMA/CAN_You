using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VaultsOfTheElixir.Core;

public class LevelManagerTests
{
    private GameObject _saveManagerGO;
    private GameObject _levelManagerGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Fresh SaveManager for every test, so tests never leak state into each other.
        _saveManagerGO = new GameObject("_SaveManager_Test");
        _saveManagerGO.AddComponent<SaveManager>();
        yield return null; // let Awake() run

        SaveManager.Instance.ResetSave();

        // Fresh LevelManager for every test.
        _levelManagerGO = new GameObject("_LevelManager_Test");
        _levelManagerGO.AddComponent<LevelManager>();
        yield return null; // let Awake() run
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_levelManagerGO);
        Object.Destroy(_saveManagerGO);
        yield return null;
    }

    // ---------- Default save state (1-6) ----------

    [Test]
    public void NewSave_Vault0_IsAvailable()
    {
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(0));
    }

    [Test]
    public void NewSave_Vault1_IsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(1));
    }

    [Test]
    public void NewSave_Vault2_IsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(2));
    }

    [Test]
    public void NewSave_Vault3_IsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(3));
    }

    [Test]
    public void NewSave_Vault4_IsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(4));
    }

    [Test]
    public void NewSave_AllVaults_MatchExpectedDefaultPattern()
    {
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(0));
        for (int i = 1; i <= 4; i++)
            Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(i));
    }

    // ---------- GetStatus bounds checking (7-8) ----------

    [Test]
    public void GetStatus_NegativeIndex_ReturnsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(-1));
    }

    [Test]
    public void GetStatus_IndexTooLarge_ReturnsLocked()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(99));
    }

    // ---------- CanAccessLevel (9-10) ----------

    [Test]
    public void CanAccessLevel_Vault0_TrueByDefault()
    {
        Assert.IsTrue(LevelManager.Instance.CanAccessLevel(0));
    }

    [Test]
    public void CanAccessLevel_Vault1_FalseByDefault()
    {
        Assert.IsFalse(LevelManager.Instance.CanAccessLevel(1));
    }

    // ---------- HasCollectedRelic default state (11) ----------

    [Test]
    public void HasCollectedRelic_AllVaults_FalseByDefault()
    {
        for (int i = 0; i <= 4; i++)
            Assert.IsFalse(LevelManager.Instance.HasCollectedRelic(i));
    }

    // ---------- CollectRelic core behavior (12-15) ----------

    [Test]
    public void CollectRelic_Vault0_MarksCompleted()
    {
        LevelManager.Instance.CollectRelic(0);
        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(0));
    }

    [Test]
    public void CollectRelic_Vault0_UnlocksVault1()
    {
        LevelManager.Instance.CollectRelic(0);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(1));
    }

    [Test]
    public void CollectRelic_Vault0_AddsToRelicsCollected()
    {
        LevelManager.Instance.CollectRelic(0);
        Assert.IsTrue(LevelManager.Instance.HasCollectedRelic(0));
    }

    [Test]
    public void CollectRelic_CalledTwice_DoesNotDuplicateOrBreak()
    {
        LevelManager.Instance.CollectRelic(0);
        LevelManager.Instance.CollectRelic(0);

        Assert.IsTrue(LevelManager.Instance.HasCollectedRelic(0));
        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(0));
    }

    // ---------- Sequential vault progression (16-19) ----------

    [Test]
    public void CollectRelic_Vault1_DirectCall_StillMarksCompleted()
    {
        // Documents current behavior: CollectRelic has no access-control check,
        // so it can be called even on a vault that's technically still Locked.
        LevelManager.Instance.CollectRelic(1);
        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(1));
    }

    [Test]
    public void CollectRelic_Vault1_UnlocksVault2()
    {
        LevelManager.Instance.CollectRelic(0);
        LevelManager.Instance.CollectRelic(1);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(2));
    }

    [Test]
    public void CollectRelic_Vault2_UnlocksVault3()
    {
        LevelManager.Instance.CollectRelic(0);
        LevelManager.Instance.CollectRelic(1);
        LevelManager.Instance.CollectRelic(2);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(3));
    }

    [Test]
    public void FullChain_Vault0Through2_SequentiallyUnlocksEachNext()
    {
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(1));
        LevelManager.Instance.CollectRelic(0);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(1));

        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(2));
        LevelManager.Instance.CollectRelic(1);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(2));

        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(3));
        LevelManager.Instance.CollectRelic(2);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(3));
    }

    // ---------- Vault 3 -> Vault 4 special-case rule (20-22) ----------

    [Test]
    public void CollectRelic_Vault3_ForceUnlocksVault4_EvenAlone()
    {
        // Vault 3 completion always force-unlocks Vault 4, regardless of
        // whether earlier vaults were completed or how many relics exist.
        LevelManager.Instance.CollectRelic(3);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(4));
    }

    [Test]
    public void CollectRelic_Vault3_MarksVault3Completed()
    {
        LevelManager.Instance.CollectRelic(3);
        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(3));
    }

    [Test]
    public void FullChain_AllFourRelicsThenVault3_Vault4Available()
    {
        LevelManager.Instance.CollectRelic(0);
        LevelManager.Instance.CollectRelic(1);
        LevelManager.Instance.CollectRelic(2);
        LevelManager.Instance.CollectRelic(3);
        Assert.AreEqual(LevelStatus.Available, LevelManager.Instance.GetStatus(4));
    }

    // ---------- Vault 4 / final vault edge cases (23-26) ----------

    [Test]
    public void CollectRelic_Vault4_MarksCompleted()
    {
        LevelManager.Instance.CollectRelic(4);
        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(4));
    }

    [Test]
    public void CollectRelic_Vault4_DoesNotThrow_WhenNoGameManagerPresent()
    {
        // Vault 4 is the final vault, so MarkLevelCompleted returns before
        // attempting any scene load - this should never throw even with
        // no GameManager in the test scene.
        Assert.DoesNotThrow(() => LevelManager.Instance.CollectRelic(4));
    }

    [Test]
    public void CollectRelic_Vault4_DoesNotAffectOutOfRangeIndex()
    {
        LevelManager.Instance.CollectRelic(4);
        // Index 5 is out of range and should safely report Locked, not throw.
        Assert.AreEqual(LevelStatus.Locked, LevelManager.Instance.GetStatus(5));
    }

    [Test]
    public void HasCollectedRelic_TrueAfterCollectingVault4()
    {
        LevelManager.Instance.CollectRelic(4);
        Assert.IsTrue(LevelManager.Instance.HasCollectedRelic(4));
    }

    // ---------- CanAccessLevel after progression (27-28) ----------

    [Test]
    public void CanAccessLevel_AfterCollectingVault0_Vault1BecomesTrue()
    {
        Assert.IsFalse(LevelManager.Instance.CanAccessLevel(1));
        LevelManager.Instance.CollectRelic(0);
        Assert.IsTrue(LevelManager.Instance.CanAccessLevel(1));
    }

    [Test]
    public void CanAccessLevel_Vault4_FalseUntilUnlockedByVault3()
    {
        Assert.IsFalse(LevelManager.Instance.CanAccessLevel(4));
        LevelManager.Instance.CollectRelic(3);
        Assert.IsTrue(LevelManager.Instance.CanAccessLevel(4));
    }

    // ---------- Relic tracking across multiple vaults (29-30) ----------

    [Test]
    public void RelicsCollected_MultipleVaults_AllTrackedIndependently()
    {
        LevelManager.Instance.CollectRelic(0);
        LevelManager.Instance.CollectRelic(2);

        Assert.IsTrue(LevelManager.Instance.HasCollectedRelic(0));
        Assert.IsFalse(LevelManager.Instance.HasCollectedRelic(1));
        Assert.IsTrue(LevelManager.Instance.HasCollectedRelic(2));
        Assert.IsFalse(LevelManager.Instance.HasCollectedRelic(3));
        Assert.IsFalse(LevelManager.Instance.HasCollectedRelic(4));
    }

    [Test]
    public void CollectRelic_RepeatedCalls_VaultStatusStaysCompletedNotReset()
    {
        LevelManager.Instance.CollectRelic(1);
        LevelManager.Instance.CollectRelic(1);
        LevelManager.Instance.CollectRelic(1);

        Assert.AreEqual(LevelStatus.Completed, LevelManager.Instance.GetStatus(1));
    }
}