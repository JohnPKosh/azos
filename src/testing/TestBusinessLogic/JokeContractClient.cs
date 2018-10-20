using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Glue;
using Azos.Glue.Protocol;

namespace TestBusinessLogic
{
  ///<summary>
  /// Client for glued contract BusinessLogic.IJokeContract server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class JokeContractClient : ClientEndPoint, @BusinessLogic.@IJokeContract
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Echo_0;
     private static MethodSpec @s_ms_UnsecureEcho_1;
     private static MethodSpec @s_ms_UnsecEchoMar_2;
     private static MethodSpec @s_ms_SimpleWorkAny_3;
     private static MethodSpec @s_ms_SimpleWorkMar_4;
     private static MethodSpec @s_ms_DBWork_5;
     private static MethodSpec @s_ms_Notify_6;
     private static MethodSpec @s_ms_ObjectWork_7;

     //static .ctor
     static JokeContractClient()
     {
         var t = typeof(@BusinessLogic.@IJokeContract);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Echo_0 = new MethodSpec(t.GetMethod("Echo", new Type[]{ typeof(@System.@String) }));
         @s_ms_UnsecureEcho_1 = new MethodSpec(t.GetMethod("UnsecureEcho", new Type[]{ typeof(@System.@String) }));
         @s_ms_UnsecEchoMar_2 = new MethodSpec(t.GetMethod("UnsecEchoMar", new Type[]{ typeof(@System.@String) }));
         @s_ms_SimpleWorkAny_3 = new MethodSpec(t.GetMethod("SimpleWorkAny", new Type[]{ typeof(@System.@String), typeof(@System.@Int32), typeof(@System.@Int32), typeof(@System.@Boolean), typeof(@System.@Double) }));
         @s_ms_SimpleWorkMar_4 = new MethodSpec(t.GetMethod("SimpleWorkMar", new Type[]{ typeof(@System.@String), typeof(@System.@Int32), typeof(@System.@Int32), typeof(@System.@Boolean), typeof(@System.@Double) }));
         @s_ms_DBWork_5 = new MethodSpec(t.GetMethod("DBWork", new Type[]{ typeof(@System.@String), typeof(@System.@Int32), typeof(@System.@Int32) }));
         @s_ms_Notify_6 = new MethodSpec(t.GetMethod("Notify", new Type[]{ typeof(@System.@String) }));
         @s_ms_ObjectWork_7 = new MethodSpec(t.GetMethod("ObjectWork", new Type[]{ typeof(@System.@Object) }));
     }
  #endregion

  #region .ctor
     public JokeContractClient(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public JokeContractClient(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public JokeContractClient(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public JokeContractClient(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@BusinessLogic.@IJokeContract); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.Echo'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @Echo(@System.@String  @text)
         {
            var call = Async_Echo(@text);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.Echo'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Echo(@System.@String  @text)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Echo_0, false, RemoteInstance, new object[]{@text});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.UnsecureEcho'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @UnsecureEcho(@System.@String  @text)
         {
            var call = Async_UnsecureEcho(@text);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.UnsecureEcho'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_UnsecureEcho(@System.@String  @text)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_UnsecureEcho_1, false, RemoteInstance, new object[]{@text});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.UnsecEchoMar'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @UnsecEchoMar(@System.@String  @text)
         {
            var call = Async_UnsecEchoMar(@text);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.UnsecEchoMar'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_UnsecEchoMar(@System.@String  @text)
         {
            var request = new @BusinessLogic.@RequestMsg_IJokeContract_UnsecEchoMar(s_ts_CONTRACT, @s_ms_UnsecEchoMar_2, false, RemoteInstance)
            {
               MethodArg_0_text = @text,
            };
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.SimpleWorkAny'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @SimpleWorkAny(@System.@String  @s, @System.@Int32  @i1, @System.@Int32  @i2, @System.@Boolean  @b, @System.@Double  @d)
         {
            var call = Async_SimpleWorkAny(@s, @i1, @i2, @b, @d);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.SimpleWorkAny'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_SimpleWorkAny(@System.@String  @s, @System.@Int32  @i1, @System.@Int32  @i2, @System.@Boolean  @b, @System.@Double  @d)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_SimpleWorkAny_3, false, RemoteInstance, new object[]{@s, @i1, @i2, @b, @d});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.SimpleWorkMar'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@String' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@String @SimpleWorkMar(@System.@String  @s, @System.@Int32  @i1, @System.@Int32  @i2, @System.@Boolean  @b, @System.@Double  @d)
         {
            var call = Async_SimpleWorkMar(@s, @i1, @i2, @b, @d);
            return call.GetValue<@System.@String>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.SimpleWorkMar'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_SimpleWorkMar(@System.@String  @s, @System.@Int32  @i1, @System.@Int32  @i2, @System.@Boolean  @b, @System.@Double  @d)
         {
            var request = new @BusinessLogic.@RequestMsg_IJokeContract_SimpleWorkMar(s_ts_CONTRACT, @s_ms_SimpleWorkMar_4, false, RemoteInstance)
            {
               MethodArg_0_s = @s,
               MethodArg_1_i1 = @i1,
               MethodArg_2_i2 = @i2,
               MethodArg_3_b = @b,
               MethodArg_4_d = @d,
            };
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.DBWork'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Object' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Object @DBWork(@System.@String  @id, @System.@Int32  @recCount, @System.@Int32  @waitMs)
         {
            var call = Async_DBWork(@id, @recCount, @waitMs);
            return call.GetValue<@System.@Object>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.DBWork'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_DBWork(@System.@String  @id, @System.@Int32  @recCount, @System.@Int32  @waitMs)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_DBWork_5, false, RemoteInstance, new object[]{@id, @recCount, @waitMs});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.Notify'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         ///</summary>
         public void @Notify(@System.@String  @text)
         {
            var call = Async_Notify(@text);
            if (call.CallStatus != CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus, "Call failed: 'JokeContractClient.Notify'");
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.Notify'.
         /// This is a one-way call per contract specification, meaning - the server sends no acknowledgement of this call receipt and
         /// there is no result that server could return back to the caller.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg.
         ///</summary>
         public CallSlot Async_Notify(@System.@String  @text)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Notify_6, true, RemoteInstance, new object[]{@text});
            return DispatchCall(request);
         }



         ///<summary>
         /// Synchronous invoker for  'BusinessLogic.IJokeContract.ObjectWork'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Object' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Object @ObjectWork(@System.@Object  @dummy)
         {
            var call = Async_ObjectWork(@dummy);
            return call.GetValue<@System.@Object>();
         }

         ///<summary>
         /// Asynchronous invoker for  'BusinessLogic.IJokeContract.ObjectWork'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_ObjectWork(@System.@Object  @dummy)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_ObjectWork_7, false, RemoteInstance, new object[]{@dummy});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
