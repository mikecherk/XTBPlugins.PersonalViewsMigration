﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Carfup.XTBPlugins.AppCode
{
    public class UserManager
    {
        #region Variables

        /// <summary>
        /// Crm web service
        /// </summary>

        public ControllerManager controller = null;



        #endregion Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the class Controller manager
        /// </summary>
        /// <param name="connection">Controller manager</param>
        public UserManager(ControllerManager controller)
        {
            this.controller = controller;
        }

        #endregion Constructor

        #region Methods

        
        public Boolean ManageImpersonification(bool action = false, Guid? userGuid = null)
        {
            if (userGuid == null)
                userGuid = controller.userDestination.Value;

            bool isUserModified = action;

            if (isUserModified)
            {
                CheckIfUserEnabled(userGuid.Value, 0);
            }
            else
            {
                isUserModified = CheckIfUserEnabled(userGuid.Value);
            }

            controller.UpdateCallerId(userGuid.Value);

            return isUserModified;
        }

        private Boolean CheckIfUserEnabled(Guid userGuid, int accessmode = 4)
        {
            bool ismodified = false;

            // We put back the admin user to be sure that he has permission to perform the following actions
            controller.UpdateCallerId(controller.XTBUser.Value);

            // By default i set it to the user
            Entity user = this.controller.service.Retrieve("systemuser", userGuid, new ColumnSet("isdisabled", "accessmode", "fullname"));

            //if user is null or is onprem, no need to manage the non interactive mode                    
            if (user == null || controller.isOnPrem)
                return ismodified;

            // we check if the user exist in the crm  
            Trace.TraceInformation($"checking User : {user["fullname"]}, isdisabled : {user["isdisabled"]}, accessmode : {((OptionSetValue) user["accessmode"]).Value}");
            // If the user is disabled or is in Non Interactive mode, we update it.
            if (Boolean.Parse(user["isdisabled"].ToString()) || ((OptionSetValue)user["accessmode"]).Value == 4)
            {
                user["accessmode"] = new OptionSetValue(accessmode);

                controller.service.Update(user);
                Trace.TraceInformation($"updated User : {user["fullname"]} to accessmode : {accessmode}");
                ismodified = true;
            }

            return ismodified;
        }
        
        public List<Entity> GetListOfUsers()
        {
            return controller.service.RetrieveMultiple(new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("domainname", "firstname", "lastname", "systemuserid", "isdisabled"),
                
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("accessmode", ConditionOperator.NotEqual, 3),
                        new ConditionExpression("domainname", ConditionOperator.NotNull),
                        new ConditionExpression("domainname", ConditionOperator.NotEqual, ""),
                        new ConditionExpression("domainname", ConditionOperator.NotIn, new string[] {"bap_sa@microsoft.com", "crmoln2@microsoft.com"}),
                    }, 
                    FilterOperator = LogicalOperator.And
                }
            }).Entities.ToList();
        }

        public bool UserHasAnyRole(Guid userId)
        {
            var retrieveRoles = controller.service.RetrieveMultiple(new QueryExpression("systemuserroles")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                    }
                }
            }).Entities.ToList();

            return retrieveRoles.Any();
        }

        public bool CheckIfNonInteractiveSeatAvailable()
        {
            var nonInteractiveCount = controller.service.RetrieveMultiple(new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("accessmode", ConditionOperator.Equal, 4)                        
                    }
                }
            }).Entities.Count;

            return nonInteractiveCount < 5;
        }
        #endregion Methods
    }
}
