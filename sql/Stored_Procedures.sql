--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: November 2010                   *
--*                                                                                               *
--* This file creates the Stored Procedures used by Rummage.                                      *
--*                                                                                               *
--*************************************************************************************************
SET TERM ^ ;
ALTER PROCEDURE INSERTCONTAINER (
    CONTAINERURL Varchar(8192),
    CONTAINERTYPECODE Char(3) )
RETURNS (
    NEWID Integer )
AS
declare containerCount int;
declare containerTypeId int;
declare newContainerId int;
begin
   select container_type_id from container_type where code = :containerTypeCode into :containerTypeId;
   select count(*) from containers 
    where container = :containerURL 
      and container_type_id = :containerTypeId
    into :containerCount;
   if (:containerCount = 0) 
   then begin
   
    select coalesce(max(container_id) + 1,1) from containers into :newContainerId;
	insert into containers (container_id, container, container_type_id) 
	       	    	values (:newContainerId, trim(:containerURL), :containerTypeId);
   end
   select container_id from containers where container = :containerURL into :newId;
   
end^

SET TERM ; ^
GRANT EXECUTE
 ON PROCEDURE INSERTCONTAINER TO  XANTHALAS;
 
SET TERM ^ ;

alter procedure InsertSearchRequest (name varchar(64))
returns (newId int)
as

begin

    insert into search_request (search_request_id, name, date_request_created, date_search_last_run)
     values ((select coalesce(max(sr2.search_request_id) + 1 , 1) from search_request sr2), :name, current_date, null);
   
   select max(search_request_id) from search_request where name = :name into :newId;
   
end^

SET TERM ; ^
