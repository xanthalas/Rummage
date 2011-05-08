--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: November 2010                   *
--*                                                                                               *
--* This file creates the Stored Procedures used by Rummage.                                      *
--*                                                                                               *
--*************************************************************************************************

--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: November 2010                   *
--* Name:   INSERTCONTAINER                                                                       *
--*                                                                                               *
--* Stored Procedure which inserts a new container. Duplicates are not permitted so if the        *
--* container about to be added is already present the insert is skipped and the id of the        *
--* existing container is returned.                                                               *
--*************************************************************************************************
SET TERM ^ ;
create PROCEDURE INSERTCONTAINER (CONTAINERURL Varchar(8192), CONTAINERTYPECODE Char(3) )
                                 RETURNS (NEWID Integer )
AS
declare containerCount int;
declare containerTypeId int;
declare newContainerId int;
begin
   select container_type_id from CONTAINER_TYPE where code = :containerTypeCode into :containerTypeId;
   select count(*) from CONTAINER 
    where container_url = :containerURL 
      and container_type_id = :containerTypeId
    into :containerCount;
   if (:containerCount = 0) 
   then begin
   
    select coalesce(max(container_id) + 1,1) from CONTAINER into :newContainerId;
	insert into CONTAINER (container_id, container_url, container_type_id) 
	       	    	values (:newContainerId, trim(:containerURL), :containerTypeId);
   end
   select container_id from CONTAINER where container_url = :containerURL into :newId;
   
end^

/* Test command:
    execute procedure INSERTCONTAINER 'd:\code', 'DIR';

*/

SET TERM ; ^
GRANT EXECUTE
 ON PROCEDURE INSERTCONTAINER TO  XANTHALAS;
 
--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: November 2010                   *
--* Name:   INSERTSEARCHREQUEST                                                                   *
--*                                                                                               *
--* Stored Procedure which inserts a new search request.                                          *
--*************************************************************************************************
SET TERM ^ ;

create procedure InsertSearchRequest (  GUID Varchar(64), 
                                        NAME varchar(64),
                                        CASESENSITIVE Char(1),
                                        SEARCHHIDDEN Char(1),
                                        SEARCHBINARIES Char(1),
                                        RECURSE Char(1))
                                     returns (newId int)
as

begin

    insert into SEARCH_REQUEST (    search_request_id, 
                                    search_request_guid, 
                                    name, 
                                    case_sensitive, 
                                    search_hidden, 
                                    search_binaries, 
                                    recurse ,
                                    date_request_created, 
                                    date_search_last_run    )
     values (                       (select coalesce(max(sr2.search_request_id) + 1 , 1) from SEARCH_REQUEST sr2), 
                                    :GUID, 
                                    :NAME, 
                                    :CASESENSITIVE, 
                                    :SEARCHHIDDEN, 
                                    :SEARCHBINARIES, 
                                    :RECURSE, 
                                    current_timestamp, 
                                    current_timestamp    );
   
   select max(search_request_id) from SEARCH_REQUEST where name = :name into :newId;
   
end^
/* Test command:
    execute procedure INSERTSEARCHREQUEST 'Request1';

*/
SET TERM ; ^
--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: November 2010                   *
--* Name:   INSERTSEARCHTERM                                                                      *
--*                                                                                               *
--* Stored Procedure which inserts a new search term. If the term is already present then no      *
--* insert is done and the id of the existing term is returned.                                   *
--*************************************************************************************************
SET TERM ^ ;

create procedure InsertSearchTerm (searchTerm varchar(1024))
                                   RETURNS (NEWID Integer )
as

declare termCount int;
declare newTermId int;
begin
   select count(*) from SEARCH_TERM 
    where search_term = :searchTerm 
    into :termCount;
   if (:termCount = 0) 
   then begin
   
        select coalesce(max(search_term_id) + 1,1) from SEARCH_TERM into :newTermId;

        insert into SEARCH_TERM (search_term_id, search_term)
                        values (:newTermId, :searchTerm);
   end
   
   select search_term_id from SEARCH_TERM where search_term = :searchTerm into :newId;
   
end^

/* Test command:
    execute procedure INSERTSEARCHTERM 'Term 1';

*/
SET TERM ; ^
--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: December 2010                   *
--* Name:   INSERTSEARCHREQUESTCONTAINER                                                          *
--*                                                                                               *
--* Stored Procedure which links a search container to a search request.                          *
--*************************************************************************************************
SET TERM ^ ;

create procedure InsertSearchRequestContainer (searchRequestId Integer, searchContainerId Integer)
as

begin
        insert into SEARCH_REQUEST_CONTAINER (search_request_id, container_id)
                        values (:searchRequestId, :searchContainerId);
end^

/* Test command:
    execute procedure INSERTSEARCHREQUESTCONTAINER 1, 1;

*/
SET TERM ; ^
--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: February 2011                   *
--* Name:   INSERTSEARCHREQUESTTERM                                                               *
--*                                                                                               *
--* Stored Procedure which links a search term to a search request.                               *
--*************************************************************************************************
SET TERM ^ ;
create procedure InsertSearchRequestTerm (searchRequestId Integer, searchTermId Integer)

as

begin
        insert into SEARCH_REQUEST_TERM (search_request_id, search_term_id)
                        values (:searchRequestId, :searchTermId);
end^

/* Test command:
    execute procedure INSERTSEARCHREQUESTTERM 1, 1;
*/

SET TERM ; ^
