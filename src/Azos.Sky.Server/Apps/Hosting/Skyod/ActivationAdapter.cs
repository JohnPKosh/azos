﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Apps.Hosting.Skyod
{

  /// <summary>
  /// Outlines protocol for activities related to software component activation and lifetime management
  /// </summary>
  public abstract class ActivationAdapter : AdapterBase
  {
    protected ActivationAdapter(SetComponent director) : base(director)
    {
    }


    public override IEnumerable<Type> SupportedRequestTypes
    {
      get
      {
        yield return typeof(ActivationStartRequest);
        yield return typeof(ActivationStopRequest);
      }
    }

    protected sealed override async Task<AdapterResponse> DoExecRequestAsync(AdapterRequest request)
    {
      ComponentDirector.IsManagedActivation.IsTrue("Support managed activation");
      var response = await DoExecActivationRequest(request.CastTo<ActivationRequest>());
      return response;
    }

    protected abstract Task<ActivationResponse> DoExecActivationRequest(ActivationRequest request);

  }

  public abstract class ActivationRequest : AdapterRequest{ }
  public abstract class ActivationResponse : AdapterResponse { }

  [Bix("c38107b3-39b9-44fb-af98-44dcbb4cd0b7")]
  public class ActivationStartRequest : ActivationRequest { }

  [Bix("d4beeea6-c981-483d-84bc-017ba19ab022")]
  public class ActivationStartResponse : ActivationResponse{ }

  [Bix("541cab59-3834-4c58-980e-f8e6a12980ec")]
  public class ActivationStopRequest : ActivationRequest{ }

  [Bix("ac5c1b00-6e1f-42b7-a07e-08496e6d9180")]
  public class ActivationStopResponse : ActivationResponse { }
}
