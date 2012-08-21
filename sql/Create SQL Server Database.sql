--*************************************************************************************************
--* System: Rummage                                         Type: SQL Server T-SQL                *
--* Author: Xanthalas                                       Date: August 2011                     *
--*                                                                                               *
--* This file creates the Database used by Rummage.                                               *
--*                                                                                               *
--*************************************************************************************************

if not exists (select 1 from sys.databases where name = 'Rummage')
begin
	create database Rummage
end


