/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
use master
go
drop database if exists LaboralKutxa
go
create database LaboralKutxa
go
alter database LaboralKutxa set trustworthy on
go
use LaboralKutxa
go
create assembly LKParser from '<Ruta>\LKParser.dll'
with permission_set = unsafe

create assembly ParserSQLCLRWrapper from '<Ruta>\ParserSQLCLRWrapper.dll'
with permission_set = unsafe
go
create procedure [dbo].[Parser]
@textToParse NVARCHAR (MAX) NULL, @label NVARCHAR (MAX) NULL
AS EXTERNAL NAME [ParserSQLCLRWrapper].[StoredProcedures].[Parser]
GO
create table TrackingParser (ParsingId int identity(1, 1) primary key clustered, ParsingTs datetime2(2) default getdate(), InputText nvarchar(max), StatementText nvarchar(max), Label nvarchar(200), Alias nvarchar(max), TargetTable nvarchar(max), SourceTables nvarchar(max), Features smallint)
exec Parser @textToParse = 'UPDATE AliasDos set columna1 = 3 FROM tablaUno as Alias INNER JOIN tablaDos as AliasDos on tablaUno.ccolumna1= tablaDos.columna1 where 1 = 2;select a into mialias from tabla as mialias inner join b on a.c1 = b.c1 where a = 2; create table a (c1 int)', @label = 'etiqueta'
exec Parser @textToParse = 'CREATE VIEW Vista AS SELECT * FROM Tabla WHERE columna = 3', @label = 'otraetiqueta'
select * from TrackingParser
