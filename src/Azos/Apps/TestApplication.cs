/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Time;
using Azos.Apps.Volatile;
using Azos.Data.Access;
using Azos.Conf;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Security;
using Azos.Throttling;

namespace Azos.Apps
{
    /// <summary>
    /// Application designated for use in various unit test cases.
    /// This class is not intended for use in non-test systems
    /// </summary>
    public class TestApplication : DisposableObject, IApplication
    {
        protected Guid m_InstanceID = Guid.NewGuid();
        protected List<IConfigSettings> m_ConfigSettings = new List<IConfigSettings>();
        protected ConfigSectionNode m_ConfigRoot;
        protected ConfigSectionNode m_CommandArgs;

        public TestApplication(ConfigSectionNode cfgRoot = null)
        {
            this.ConfigRoot = cfgRoot;

            Active = true;
            StartTime = DateTime.Now;
            Log = NOPLog.Instance;
            Instrumentation = NOPInstrumentation.Instance;
            DataStore = NOPDataStore.Instance;
            ObjectStore = NOPObjectStore.Instance;
            Glue = NOPGlue.Instance;
            ModuleRoot = NOPModule.Instance;
            SecurityManager = NOPSecurityManager.Instance;
            TimeSource = Azos.Time.DefaultTimeSource.Instance;
            TimeLocation = new Time.TimeLocation();
            EventTimer = Azos.Time.NOPEventTimer.Instance;

            Realm = new ApplicationRealmBase();

            ApplicationModel.ExecutionContext.__BindApplication(this);
        }

        protected override void Destructor()
        {
            ApplicationModel.ExecutionContext.__UnbindApplication(this);
        }

        public virtual bool IsUnitTest { get; set; }

        public virtual string EnvironmentName { get; set; }

        public virtual IApplicationRealm Realm{ get; set;}

        public virtual bool ForceInvariantCulture { get; set; }

        public virtual Guid InstanceID { get { return m_InstanceID;}}

        public virtual bool AllowNesting { get { return false;}}

        public virtual DateTime StartTime { get; set;}

        public virtual bool Active { get; set; }

        public virtual bool Stopping { get; set; }

        public virtual bool ShutdownStarted { get; set; }

        public virtual Log.ILog Log { get; set; }

        public virtual Instrumentation.IInstrumentation Instrumentation { get; set; }

        public virtual IConfigSectionNode ConfigRoot
        {
           get{return m_ConfigRoot;}
           set
           {
             if (value==null)
             {
                var conf = new MemoryConfiguration();
                conf.Create();
                value = conf.Root;
             }
             m_ConfigRoot = (ConfigSectionNode)value;
           }
        }

        public virtual IConfigSectionNode CommandArgs
        {
           get{return m_CommandArgs;}
           set
           {
             if (value==null)
             {
                var conf = new MemoryConfiguration();
                conf.Create();
                value = conf.Root;
             }
             m_CommandArgs = (ConfigSectionNode)value;
           }
        }



        public virtual IDataStore DataStore { get; set; }

        public virtual Volatile.IObjectStore ObjectStore { get; set; }

        public virtual Glue.IGlue Glue { get; set; }

        public virtual IModule ModuleRoot { get; set; }

        public virtual Security.ISecurityManager SecurityManager { get; set; }

        public virtual Time.ITimeSource TimeSource { get; set; }

        public virtual Time.IEventTimer EventTimer { get; set; }

        public virtual ISession MakeNewSessionInstance(Guid sessionID, Security.User user = null)
        {
            return NOPSession.Instance;
        }

        /// <summary>
        /// Registers an instance of IConfigSettings with application container to receive a call when
        ///  underlying app configuration changes
        /// </summary>
        public virtual bool RegisterConfigSettings(IConfigSettings settings)
        {
            lock (m_ConfigSettings)
                if (!m_ConfigSettings.Contains(settings, Collections.ReferenceEqualityComparer<IConfigSettings>.Instance))
                {
                    m_ConfigSettings.Add(settings);
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Removes the registration of IConfigSettings from application container
        /// </summary>
        /// <returns>True if settings instance was found and removed</returns>
        public virtual bool UnregisterConfigSettings(IConfigSettings settings)
        {
            lock (m_ConfigSettings)
                return m_ConfigSettings.Remove(settings);
        }

        /// <summary>
        /// Forces notification of all registered IConfigSettings-implementers about configuration change
        /// </summary>
        public virtual void NotifyAllConfigSettingsAboutChange()
        {
            NotifyAllConfigSettingsAboutChange(m_ConfigRoot);
        }


        public virtual string Name { get; set; }

        public virtual Time.TimeLocation TimeLocation { get; set; }

        public virtual DateTime LocalizedTime
        {
            get { return UniversalTimeToLocalizedTime(TimeSource.UTCNow); }
            set { }
        }

            public DateTime UniversalTimeToLocalizedTime(DateTime utc)
            {
                if (utc.Kind!=DateTimeKind.Utc)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(utc + TimeLocation.UTCOffset, DateTimeKind.Local);
                }
                else
                {
                   return TimeSource.UniversalTimeToLocalizedTime(utc);
                }
            }

            public DateTime LocalizedTimeToUniversalTime(DateTime local)
            {
                if (local.Kind!=DateTimeKind.Local)
                 throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

                var loc = TimeLocation;
                if (!loc.UseParentSetting)
                {
                   return DateTime.SpecifyKind(local - TimeLocation.UTCOffset, DateTimeKind.Utc);
                }
                else
                {
                   return TimeSource.LocalizedTimeToUniversalTime(local);
                }
            }

        /// <summary>
        /// Forces notification of all registered IConfigSettings-implementers about configuration change
        /// </summary>
        protected void NotifyAllConfigSettingsAboutChange(IConfigSectionNode node)
        {
            node = node ?? m_ConfigRoot;

            lock (m_ConfigSettings)
                foreach (var s in m_ConfigSettings) s.ConfigChanged(node);
        }


        public bool RegisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public bool UnregisterAppFinishNotifiable(IApplicationFinishNotifiable notifiable)
        {
            return false;
        }

        public void Stop()
        {

        }
    }
}
