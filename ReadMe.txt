Appendix B. ReadMe.txt

Version: 2.1.6, September 13, 2018

Introduction
DevTreks is a multitier ASP.NET Core 2 database 
application. The web project, DevTreks, uses an 
MVC pattern. The data layer, DevTreks.Data, uses 
an EF Core 2 data repository pattern. EF data models 
are stored in the DevTreks.Models project. ASPNET 
Identity models are stored in the DevTreks web 
Project’s Data folder. Localization strings are stored in 
the DevTreks.Exceptions and DevTreks.Resources 
projects. The DevTreks.Extensions folder holds 
projects that use a Managed Extensibility Framework 
pattern. Each project holds a separate group of 
calculators and analyzers. 

Always visit the What's New link on the home site 
for the latest news. The What's New text file lists 
tutorials that have been upgraded recently. Those 
tutorials are usually associated with the current 
release. The Source Code tutorial explains how the 
source code works. The Social Budgeting tutorial 
explains how to manage networks, clubs, and 
members to deliver social budgeting data services. 
The Calculators and Analyzers tutorial explains 
how calculators and analyzers work. 

home site
https://www.devtreks.org

source code sites
https://github.com/kpboyle1/devtreks2.1 (.NET Core 2.1)

database.zip file
https://devtreks1.blob.core.windows.net/resources/db216.zip

214 datafiles (any exceeding 500KB must be manually uploaded)
https://devtreks1.blob.core.windows.net/resources/network_carbon.zip

What's New in Version 2.1.6
1. This release upgraded to AspNet Core 2.1, fixed bugs found during further 2.1.4 testing, and upgraded the calculator patterns.


Server version: Sql Server 2017 Express, RTM

connection string
Server=localhost\SQLEXPRESS;Database=DevTreksDesk;Trusted_Connection=True;

DevTreks default member login
Name: kpboyle1@comcast.net
Pwd: public2A@

system administrator
SqlExpress 2016 databases can be accessed using a Windows OS logged in user –these haven’t been tested with the new db server and aren’t critical for accessing the db in SSMS
User: devtreks01_sa or sa
Pwd: public





