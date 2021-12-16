﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;
using Azos.Security.MinIdp;

namespace Azos.AuthKit
{
  /// <summary>
  /// Outlines core functionality for working with user accounts.
  /// The logic is compatible with/based on MinIdp
  /// </summary>
  public interface IIdpUserCoreLogic : IBusinessLogic, IMinIdpStore
  {
    ///// <summary>
    ///// Returns a list of user accounts
    ///// </summary>
    Task<IEnumerable<UserInfo>> GetUserListAsync(UserListFilter filter);

    ///// <summary>
    ///// Returns a list of tags for the specified user account
    ///// </summary>
    //PII//Task<IEnumerable<UserTagInfo>> GetUserTagsAsync(GDID gUser);

    // <summary>
    // Returns a list of login info objects for the selected user account
    // </summary>
    Task<IEnumerable<LoginInfo>> GetLoginsAsync(GDID gUser);

    Task<ValidState> ValidateUserAsync(UserEntity user, ValidState state);
    Task<ChangeResult> SaveUserAsync(UserEntity user);

    //Task<ValidState> ValidateLoginAsync(LoginEntity login, ValidState state);
    //Task<ChangeResult> SaveLoginAsync(LoginEntity login);

    /// <summary>
    /// Invoked by EventHub reactor, pulls event from queue and applies it to the IDP.
    /// A `LoginEvent` is generated in response to successful password set, login or
    /// bad login attempt
    /// </summary>
    Task<ChangeResult> ApplyLoginEventAsync(Events.LoginEvent what);
  }
}
