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

namespace Azos.Apps.Hosting.Skyod
{
  public sealed class DefaultAzosPackageInstaller : InstallationAdapter
  {
    public DefaultAzosPackageInstaller(SetComponent director) : base(director)
    {
    }

    protected override Task<InstallationResponse> DoExecActivationRequest(InstallationRequest request)
    {
      throw new NotImplementedException();
    }
  }
}
