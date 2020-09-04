/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Log;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;
using Azos.Security.MinIdp;
using Azos.Security;
using System.Linq;
using Azos.Time;
using System.Collections.Generic;
using Azos.Conf;

namespace Azos.Data.Access.MongoDb.Security.MinIdp
{
  internal static class BsonModel
  {
    public const string FLD_GDID = "_id"; // Query._ID;
    public const string FLD_SYSID = "sysid";
    public const string FLD_REALM = "realm";

    public const string FLD_ROLE = "role";
    public const string FLD_RIGHTS = "rights";
    public const string FLD_STATUS = "stat";

    public const string FLD_CREATE_DT = "cd";
    public const string FLD_START_DT = "sd";
    public const string FLD_END_DT = "ed";

    public const string FLD_NAME = "name";
    public const string FLD_DESC = "desc";
    public const string FLD_SNAME = "sname";
    public const string FLD_NOTE = "note";

    public const string FLD_LOGIN = "login";

    public const string FLD_LOGIN_ID = "id";
    public const string FLD_LOGIN_PWD = "pwd";
    public const string FLD_LOGIN_START_DT = "sd";
    public const string FLD_LOGIN_END_DT = "ed";

    public const string FLD_ROLE_ID = "id";

    public static MinIdpRoleData RoleFromBson(BSONDocument bson)
    {
      var role = new MinIdpRoleData();
      if (bson[FLD_ROLE_ID] is BSONStringElement roleId) role.RoleId = roleId.Value;
      if (bson[FLD_REALM] is BSONInt64Element binRealm) role.Realm = new Atom((ulong)binRealm.Value); // replace with target realm by connection context
      if (bson[FLD_DESC] is BSONStringElement desc) role.Description = desc.Value;

      // refactor below
      if (bson[FLD_RIGHTS] is BSONStringElement rights) role.Rights =
          new Rights(Configuration.ProviderLoadFromString(rights.Value.AsLaconicConfig().AsString(),".laconf"));

      if (bson[FLD_START_DT] is BSONDateTimeElement sd) role.StartUtc = sd.Value;
      if (bson[FLD_END_DT] is BSONDateTimeElement ed) role.EndUtc = ed.Value;
      if (bson[FLD_NOTE] is BSONStringElement note) role.Note = note.Value;

      return role;
    }

    public static BSONDocument RoleToBson(MinIdpRoleData role)
    {
      var doc = new BSONDocument();

      // where will validation happen???

      doc.Set(new BSONStringElement(FLD_ROLE_ID, role.RoleId));
      doc.Set(new BSONInt64Element(FLD_REALM, (long)role.Realm.ID));  // replace with target realm by connection context
      if (role.Description.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_DESC));
      else
        doc.Set(new BSONStringElement(FLD_DESC, role.Description));

      doc.Set(new BSONStringElement(FLD_RIGHTS, role.Rights.AsLaconicConfig().AsString()));

      doc.Set(new BSONDateTimeElement(FLD_START_DT, role.StartUtc));
      doc.Set(new BSONDateTimeElement(FLD_END_DT, role.EndUtc));
      if (role.Note.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_NOTE));
      else
        doc.Set(new BSONStringElement(FLD_NOTE, role.Note));

      return doc;
    }

    public static MinIdpUserData UserFromBson(BSONDocument bson)
    {
      var usr = new MinIdpUserData();

      //if (bson[FLD_GDID] is BSONBinaryElement binGdid) usr.Gdid = DataDocConverter.GDID_BSONtoCLR(binGdid);
      if (bson[FLD_SYSID] is BSONInt64Element sysId) usr.SysId = (ulong)sysId.Value;
      if (bson[FLD_REALM] is BSONInt64Element binRealm) usr.Realm = new Atom((ulong)binRealm.Value); // replace with target realm by connection context
      if (bson[FLD_ROLE] is BSONStringElement role) usr.Role = role.Value;
      if (bson[FLD_STATUS] is BSONInt32Element stat) usr.Status = (UserStatus)stat.Value;

      if (bson[FLD_CREATE_DT] is BSONDateTimeElement cd) usr.CreateUtc = cd.Value;
      if (bson[FLD_START_DT] is BSONDateTimeElement sd) usr.StartUtc = sd.Value;
      if (bson[FLD_END_DT] is BSONDateTimeElement ed) usr.EndUtc = ed.Value;

      if (bson[FLD_NAME] is BSONStringElement name) usr.Name = name.Value;
      if (bson[FLD_DESC] is BSONStringElement desc) usr.Description = desc.Value;
      if (bson[FLD_SNAME] is BSONStringElement sname) usr.ScreenName = sname.Value;
      if (bson[FLD_NOTE] is BSONStringElement note) usr.Note = note.Value;

      var logins = new List<MinIdpLoginData>();   // ***Complex model example***
      if (bson[FLD_LOGIN] is BSONArrayElement loginArr)
      {
        foreach (BSONElement l in loginArr.Value)
        {
          if(l is BSONDocumentElement elm)
          {
            MinIdpLoginData login = LoginFromBson(elm.Value);
            logins.Add(login);
          }
        }
      }
      // Use App.TimeSource instead
      var activeLogin = logins.FirstOrDefault(x => x.LoginStartUtc >= DateTime.UtcNow && x.LoginEndUtc <= DateTime.UtcNow);
      if(activeLogin == null) return null;  // may need additional flag if we want to still return without login info

      // ELSE if there is an active login
      usr.LoginId = activeLogin.LoginId;
      usr.LoginPassword = activeLogin.LoginPassword;
      usr.LoginStartUtc = activeLogin.LoginStartUtc;
      usr.LoginEndUtc = activeLogin.LoginEndUtc;
      return usr;
    }

    public static BSONDocument UserToBson(MinIdpUserData usr)
    {
      var doc = new BSONDocument();

      // where will validation happen???
      doc.Set(new BSONInt64Element(FLD_SYSID, (long)usr.SysId));
      doc.Set(new BSONInt64Element(FLD_REALM, (long)usr.Realm.ID));  // replace with target realm by connection context
      doc.Set(new BSONStringElement(FLD_ROLE, usr.Role));
      doc.Set(new BSONInt32Element(FLD_STATUS, (int)usr.Status));

      doc.Set(new BSONDateTimeElement(FLD_CREATE_DT, usr.CreateUtc));
      doc.Set(new BSONDateTimeElement(FLD_START_DT, usr.StartUtc));
      doc.Set(new BSONDateTimeElement(FLD_END_DT, usr.EndUtc));

      doc.Set(new BSONStringElement(FLD_NAME, usr.Name)); // should not be null

      if (usr.Description.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_DESC));
      else
        doc.Set(new BSONStringElement(FLD_DESC, usr.Description));

      doc.Set(new BSONStringElement(FLD_SNAME, usr.ScreenName)); // should not be null

      if (usr.Note.IsNullOrWhiteSpace())
        doc.Set(new BSONNullElement(FLD_NOTE));
      else
        doc.Set(new BSONStringElement(FLD_NOTE, usr.Note));

      // Handle login data here how????  ***Flattened model example***
      doc.Set(new BSONStringElement(FLD_LOGIN_ID, usr.LoginId));
      doc.Set(new BSONStringElement(FLD_LOGIN_PWD, usr.LoginPassword));
      if (usr.LoginStartUtc.HasValue)
        doc.Set(new BSONDateTimeElement(FLD_LOGIN_START_DT, usr.LoginStartUtc.Value));
      // ELSE is needed correct?
      if (usr.LoginEndUtc.HasValue)
        doc.Set(new BSONDateTimeElement(FLD_LOGIN_END_DT, usr.LoginEndUtc.Value));
      // ELSE is needed correct?

      return doc;
    }

    public static MinIdpLoginData LoginFromBson(BSONDocument bson)
    {
      var login = new MinIdpLoginData();
      if (bson[FLD_LOGIN_ID] is BSONStringElement loginId) login.LoginId = loginId.Value;
      if (bson[FLD_LOGIN_PWD] is BSONStringElement loginPwd) login.LoginPassword = loginPwd.Value;
      if (bson[FLD_LOGIN_START_DT] is BSONDateTimeElement loginSd) login.LoginStartUtc = loginSd.Value;
      if (bson[FLD_LOGIN_END_DT] is BSONDateTimeElement loginEd) login.LoginEndUtc = loginEd.Value;
      return login;
    }

    public static BSONDocument LoginToBson(MinIdpLoginData login)
    {
      var doc = new BSONDocument();

      // where will validation happen???

      doc.Set(new BSONStringElement(FLD_LOGIN_ID, login.LoginId));
      doc.Set(new BSONStringElement(FLD_LOGIN_PWD, login.LoginPassword));
      if(login.LoginStartUtc.HasValue)
        doc.Set(new BSONDateTimeElement(FLD_LOGIN_START_DT, login.LoginStartUtc.Value));
      // ELSE is needed correct?
      if (login.LoginEndUtc.HasValue)
        doc.Set(new BSONDateTimeElement(FLD_LOGIN_END_DT, login.LoginEndUtc.Value));
      // ELSE is needed correct?

      return doc;
    }

  }
}
