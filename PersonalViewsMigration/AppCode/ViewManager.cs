﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Carfup.XTBPlugins.AppCode
{
    public class ViewManager
    {
        #region Variables

        /// <summary>
        /// Crm web service
        /// </summary>
        private readonly ControllerManager controller = null;

        private static RetrieveEntityResponse metadata = null;
        #endregion Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the class Controller manager
        /// </summary>
        /// <param name="connection">Controller manager</param>
        public ViewManager(ControllerManager connection)
        {
            this.controller = connection;
        }

        #endregion Constructor

        #region Methods

        private void RetrieveMetadataOfView()
        {
            RetrieveEntityRequest retrieveEntityAttributesRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = "userquery"
            };
            metadata = (RetrieveEntityResponse)controller.proxy.Execute(retrieveEntityAttributesRequest);
        }

        public List<Entity> ListOfUserViews(Guid userGuid)
        {
            var sharings = controller.dataManager.retrieveSharingsOfUser(userGuid, "userquery");

            var filter = new FilterExpression(LogicalOperator.Or)
            {
                Conditions =
                {
                    new ConditionExpression("userqueryid", ConditionOperator.In, sharings)
                }
            };
            if (sharings.Length == 0)
                filter = null;

            return controller.proxy.RetrieveMultiple(new QueryExpression("userquery")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.Or,
                    Filters =
                    {
                        new FilterExpression(LogicalOperator.And)
                        {
                            Conditions =
                            {
                                new ConditionExpression("ownerid", ConditionOperator.Equal, userGuid),
                                new ConditionExpression("querytype", ConditionOperator.NotIn, new[] {16,512}), // 16 = OfflineFilters, 512 = AddressBookFilters
                            }
                        },
                        filter
                    }
                },

            }).Entities.ToList();
        }

        public Entity PrepareViewToMigrate(Entity getViewDetails)
        {
            List<string> attributesList = new List<string> { "fetchxml", "returnedtypecode", "layoutxml", "querytype", "name", "advancedgroupby", "columnsetxml", "description", "offlinesqlquery" };

            if (metadata == null)
                RetrieveMetadataOfView();
            
            Entity viewToMigrate = new Entity("userquery");

            foreach(var att in attributesList)
            {
                if (metadata.EntityMetadata.Attributes.Any(x => x.LogicalName == att))
                {
                    viewToMigrate[att] = (getViewDetails.Contains(att)) ? getViewDetails[att] : null;
                }
            }

            return viewToMigrate;
        }
        #endregion Methods
    }
}
