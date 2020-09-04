/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class MinIdpBsonModelTests
  {

    public const string DESC = "I don't agree with that";
    public const string NOTE = "Walla Walla Washington";

    [Run("role='TestAdministrator' realm=1")]
    public void RoleToBson(string role, int realm)
    {
      var sut = new MinIdpRoleData()
      {
        RoleId = role,
        Realm = new Atom((ulong)realm),
        Description = DESC,
        Rights = Rights.None,
        StartUtc = DateTime.UtcNow.Date,
        EndUtc = DateTime.UtcNow.Date,
        Note = NOTE
      };

      // There is no way to access BsonModel here to unit test. Need to refactor per DK changes to MinIdp
      // Need MinIdp management interface(s) for role, user, login mocking or move testing strictly into integ tests
      // since MinIdp models are not yet exposed for public consumption?
    }

  }
}


