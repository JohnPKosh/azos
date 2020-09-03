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


    //////public static BSONDocument ToBson(Message usr)
    //////{
    //////  var doc = new BSONDocument();

    //////  doc.Set(DataDocConverter.GDID_CLRtoBSON(FLD_GDID, usr.Gdid));
    //////  doc.Set(DataDocConverter.GUID_CLRtoBSON(FLD_GUID, usr.Guid));
    //////  doc.Set(DataDocConverter.GUID_CLRtoBSON(FLD_RELATED_TO, usr.RelatedTo));

    //////  doc.Set(new BSONInt64Element(FLD_CHANNEL, (long)usr.Channel.ID));
    //////  doc.Set(new BSONInt64Element(FLD_APP, (long)usr.App.ID));
    //////  doc.Set(new BSONInt32Element(FLD_TYPE, (int)usr.Type));
    //////  doc.Set(new BSONInt32Element(FLD_SOURCE, usr.Source));
    //////  doc.Set(new BSONDateTimeElement(FLD_TIMESTAMP, usr.UTCTimeStamp));

    //////  if (usr.Host.IsNullOrWhiteSpace())
    //////    doc.Set(new BSONNullElement(FLD_HOST));
    //////  else
    //////    doc.Set(new BSONStringElement(FLD_HOST, usr.Host));

    //////  if (usr.From.IsNullOrWhiteSpace())
    //////    doc.Set(new BSONNullElement(FLD_FROM));
    //////  else
    //////    doc.Set(new BSONStringElement(FLD_FROM, usr.From));

    //////  if (usr.Topic.IsNullOrWhiteSpace())
    //////    doc.Set(new BSONNullElement(FLD_TOPIC));
    //////  else
    //////    doc.Set(new BSONStringElement(FLD_TOPIC, usr.Topic));


    //////  if (usr.Text.IsNullOrWhiteSpace())
    //////    doc.Set(new BSONNullElement(FLD_TEXT));
    //////  else
    //////    doc.Set(new BSONStringElement(FLD_TEXT, usr.Text));

    //////  if (usr.Parameters.IsNullOrWhiteSpace())
    //////    doc.Set(new BSONNullElement(FLD_PARAMETERS));
    //////  else
    //////    doc.Set(new BSONStringElement(FLD_PARAMETERS, usr.Parameters));

    //////  if (usr.ExceptionData != null)
    //////    doc.Set(new BSONStringElement(FLD_EXCEPTION, usr.ExceptionData.ToJson(JsonWritingOptions.CompactRowsAsMap)));
    //////  else
    //////    doc.Set(new BSONNullElement(FLD_EXCEPTION));

    //////  var ad = ArchiveConventions.DecodeArchiveDimensionsMap(usr);
    //////  if (ad == null)
    //////  {
    //////    doc.Set(new BSONNullElement(FLD_AD));
    //////  }
    //////  else
    //////  {
    //////    var adDoc = ad.ToBson();
    //////    doc.Set(new BSONDocumentElement(FLD_AD, adDoc));
    //////  }

    //////  return doc;
    //////}

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

      var logins = new List<MinIdpLoginData>();
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

    private static MinIdpLoginData LoginFromBson(BSONDocument bson)
    {
      var login = new MinIdpLoginData();
      if (bson[FLD_LOGIN_ID] is BSONStringElement loginId) login.LoginId = loginId.Value;
      if (bson[FLD_LOGIN_PWD] is BSONStringElement loginPwd) login.LoginPassword = loginPwd.Value;
      if (bson[FLD_LOGIN_START_DT] is BSONDateTimeElement loginSd) login.LoginStartUtc = loginSd.Value;
      if (bson[FLD_LOGIN_END_DT] is BSONDateTimeElement loginEd) login.LoginEndUtc = loginEd.Value;
      return login;
    }

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

  }
}
