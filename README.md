# XTBPlugins.PersonalViewsMigration

[![Build status](https://ci.appveyor.com/api/projects/status/rvll3u7w8gq19ryd?svg=true)](https://ci.appveyor.com/project/carfup/xtbplugins-personalviewsmigration)

This plugin allows you to migrate views from all users to others users. 

*This is only working for Online instances for now, i'm working on checking how to adapt it for OnPremise instances too.*

When an user leaves a company and had tons of views shared with a lot of people, this can be annoying to have shared views you can't get rid of because the owner left the company since too much time and even the administrator can't delete the record.

With this tool, and within the delay of 1 month (for the online version) after the user will be disabled, you will have the possibility to copy / reassign the views to someone else.

Disclaimer : To perform this action, the plugin is using the Access mode of the systemuser from Read/Write to Non Interactive in order to have access to the CRM Webservice even if the systemuser has no license.

For more details : https://stuffandtacos.azurewebsites.net/2017/12/20/migrate-personal-views-from-a-user-another/
