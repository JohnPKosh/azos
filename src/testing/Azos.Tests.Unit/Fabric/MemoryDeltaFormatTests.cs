﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Sky.Fabric;
using Azos.Sky.Fabric.Server;
using Azos.Time;

namespace Azos.Tests.Unit.Fabric
{
  [Runnable]
  public class MemoryDeltaFormatTests : IRunnableHook
  {
    public void Prologue(Runner runner, FID id)
    {
       Bixer.RegisterTypeSerializationCores(System.Reflection.Assembly.GetExecutingAssembly());
    }
    public bool Epilogue(Runner runner, FID id, Exception error) => false;

    [Run]
    public void Test01_Roundtrip_NextStep_NoResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
          Bool1 = true,
          Int1 = 123456,
          String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Booster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("donkey_fun.jpeg", new byte[]{1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5});
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

      using var wscope = BixWriterBufferScope.DefaultCapacity;
      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See();
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 223322;//mutate state <====================
      Aver.IsTrue(got.HasDelta(gotState));

      var delta = got.MakeDeltaSnapshot(FiberStep.ContinueImmediately(Atom.Encode("step2")), gotState);

      Aver.AreEqual("step2", delta.NextStep.Value);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(1, delta.Changes.Length);
      Aver.IsNull(delta.Result);
      Aver.AreEqual(0, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      using var rscope2 = new BixReaderBufferScope(wscope.Buffer);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.AreEqual("step2", gotDelta.NextStep.Value);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.AreEqual(1, gotDelta.Changes.Length);
      Aver.IsNull(gotDelta.Result);
      Aver.AreEqual(0, gotDelta.ExitCode);

      Aver.AreEqual("d", gotDelta.Changes[0].Key.Value);
      Aver.AreEqual(223322ul, gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().AccountNumber);
      Aver.AreEqual("Booster", gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().SlotMutation);
    }


    [Run]
    public void Test02_Roundtrip_FinalStep_WithResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

      using var wscope = BixWriterBufferScope.DefaultCapacity;
      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See();
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 55166;//mutate state <====================
      gotState.LastName = "Shuster";
      Aver.IsTrue(got.HasDelta(gotState));
      var result = new TeztResult
      {
        Int1 = 900,
        String1 = "Dallas"
      };

      var delta = got.MakeDeltaSnapshot(FiberStep.FinishWithResult(123, result), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(1, delta.Changes.Length);
      Aver.IsNotNull(delta.Result);
      Aver.AreEqual(123, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.AreEqual(1, gotDelta.Changes.Length);
      Aver.IsNotNull(gotDelta.Result);
      Aver.AreEqual(123, gotDelta.ExitCode);
      Aver.AreEqual(result.Int1, ((TeztResult)gotDelta.Result).Int1);
      Aver.AreEqual(result.String1, ((TeztResult)gotDelta.Result).String1);

      Aver.AreEqual("d", gotDelta.Changes[0].Key.Value);
      Aver.AreEqual(55166ul, gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().AccountNumber);
      Aver.AreEqual("Shuster", gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().SlotMutation);
    }


    [Run]
    public void Test03_Roundtrip_FinalStep_NoResult()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

      using var wscope = BixWriterBufferScope.DefaultCapacity;
      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See();
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      gotState.AccountNumber = 55166;//mutate state <====================
      gotState.LastName = "Monster";
      gotState.SetAttachment("windows.bmp", new byte[5]);
      Aver.IsTrue(gotState.SlotsHaveChanges);
      Aver.IsTrue(got.HasDelta(gotState));


      var delta = got.MakeDeltaSnapshot(FiberStep.Finish(321), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.AreEqual(2, delta.Changes.Length);
      Aver.IsNull(delta.Result);
      Aver.AreEqual(321, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.AreEqual(2, gotDelta.Changes.Length);
      Aver.IsNull(gotDelta.Result);
      Aver.AreEqual(321, gotDelta.ExitCode);

      Aver.AreEqual("d", gotDelta.Changes[0].Key.Value);
      Aver.AreEqual(55166ul, gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().AccountNumber);
      Aver.AreEqual("Monster", gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().LastName);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == gotDelta.Changes[0].Value.CastTo<TeztState.DemographicsSlot>().SlotMutation);

      Aver.AreEqual("a", gotDelta.Changes[1].Key.Value);
      Aver.AreEqual("windows.bmp", gotDelta.Changes[1].Value.CastTo<TeztState.AttachmentSlot>().AttachmentName);
      Aver.AreEqual(5, gotDelta.Changes[1].Value.CastTo<TeztState.AttachmentSlot>().AttachContent.Length);
      Aver.IsTrue(FiberState.SlotMutationType.Modified == gotDelta.Changes[1].Value.CastTo<TeztState.AttachmentSlot>().SlotMutation);
    }

    [Run]
    public void Test04_Roundtrip_Crash()
    {
      var fid = new FiberId(Atom.Encode("sys"), Atom.Encode("s1"), new GDID(7, 12, 1234567890));
      var pars = new TeztParams
      {
        Bool1 = true,
        Int1 = 123456,
        String1 = "BoltJolt"
      };

      var state = new TeztState();
      state.FirstName = "Alex";
      state.LastName = "Rooster";
      state.DOB = new DateTime(1980, 1, 2, 14, 15, 00, DateTimeKind.Utc);
      state.AccountNumber = 987654321;
      state.SetAttachment("hockey_fun.jpeg", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5 });
      state.ResetAllSlotModificationFlags();
      var mem = new FiberMemory(1, MemoryStatus.LockedForCaller, fid, Guid.NewGuid(), null, pars, state);//this packs buffer

      using var wscope = BixWriterBufferScope.DefaultCapacity;
      mem.WriteOneWay(wscope.Writer, 1);//Serialize by SHARD

      using var rscope = new BixReaderBufferScope(wscope.Buffer);
      var got = new FiberMemory(rscope.Reader);//Read by processor

      Aver.AreEqual(mem.Id, got.Id);
      Aver.IsTrue(mem.Status == got.Status);
      Aver.AreEqual(mem.ImageTypeId, got.ImageTypeId);
      Aver.AreEqual(mem.ImpersonateAs, got.ImpersonateAs);
      Aver.AreArraysEquivalent(mem.Buffer, got.Buffer);
      got.See();
      var (gotParsBase, gotStateBase) = got.UnpackBuffer(typeof(TeztParams), typeof(TeztState));
      var (gotPars, gotState) = (gotParsBase.CastTo<TeztParams>(), gotStateBase.CastTo<TeztState>());
      Aver.AreEqual(pars.Int1, gotPars.Int1);
      Aver.AreEqual(pars.String1, gotPars.String1);
      Aver.AreEqual(state.AccountNumber, gotState.AccountNumber);
      Aver.AreEqual(state.AttachmentName, gotState.AttachmentName);
      Aver.AreArraysEquivalent(state.AttachmentContent, gotState.AttachmentContent);

      //==== We are in processor pretending to use state now

      Aver.IsFalse(got.HasDelta(gotState));
      got.Crash(new FabricException("Problem X"));//crash memory

      var delta = got.MakeDeltaSnapshot(FiberStep.Finish(321), gotState);

      Aver.IsTrue(delta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), delta.NextSliceInterval);
      Aver.IsNull(delta.Changes);
      Aver.IsNull(delta.Result);
      Aver.IsNotNull(delta.Crash);
      Aver.AreEqual(0, delta.ExitCode);

      wscope.Reset();
      delta.WriteOneWay(wscope.Writer, 1);//written by processor

      var wire = wscope.Buffer;
      "{0} bytes of delta wired back to shard".SeeArgs(wire.Length);

      using var rscope2 = new BixReaderBufferScope(wire);
      var gotDelta = new FiberMemoryDelta(rscope2.Reader);//read back by shard

      gotDelta.See("Got delta: ");

      Aver.AreEqual(mem.Id, gotDelta.Id);
      Aver.AreEqual(got.Id, gotDelta.Id);


      Aver.IsTrue(gotDelta.NextStep.IsZero);
      Aver.AreEqual(new TimeSpan(0), gotDelta.NextSliceInterval);
      Aver.IsNull(gotDelta.Changes);
      Aver.IsNull(gotDelta.Result);
      Aver.AreEqual(0, gotDelta.ExitCode);
      Aver.IsNotNull(gotDelta.Crash);

      Aver.AreEqual("Problem X", gotDelta.Crash.Message);
    }
  }
}