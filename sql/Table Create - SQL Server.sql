--*************************************************************************************************
--* System: Rummage                                         Type: SQL Server - T-SQL              *
--* Author: Xanthalas                                       Date: August 2011                     *
--*                                                                                               *
--* This file contains the SQL required to create all the tables and basedata used by Rummage.    *
--*                                                                                               *
--*************************************************************************************************

--"container_type" table holds the different types of containers available to Rummage
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'container_type')
begin
	create table container_type
	(
		container_type_id int not null,
		code char(3) not null,
		name varchar(20) not null,
		
		constraint pk_container_type primary key clustered (container_type_id)

	)
end


--"container" table holds all the containers which have been searched by Rummage
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'container')
begin
	create table container (
		container_id int not null,
		container_url varchar(4096),
		container_type_id int not null,
		
		constraint pk_container primary key clustered (container_id),
		constraint fk_container_container_type foreign key (container_type_id) references container_type(container_type_id)
	)
end


--"search_term" table holds all the search terms which Rummage has searched for
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'search_term')
begin
	create table search_term
	(
		search_term_id int not null,
		search_term varchar(4096) not null,
		
		constraint pk_search_term primary key clustered (search_term_id)
	)
end

--"search_request" holds all the parameters from a single search request
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'search_request')
begin
	create table search_request 
	(
	  search_request_id int not null,
	  search_request_guid varchar(64),
	  name varchar(64),
	  case_sensitive bit,
	  search_hidden bit,
	  search_binaries bit,
	  recurse bit,
	  date_request_created smalldatetime default getdate(),
	  date_search_last_run smalldatetime default getdate(),
	  
	  constraint pk_search_request primary key clustered (search_request_id),
	)
end


--"search_request_container" holds all the containers searched for a request
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'search_request_container')
begin
	create table search_request_container
	(
		search_request_id int not null ,
		container_id int not null,
		
		constraint fk_search_request_container_request_id foreign key (search_request_id) references search_request(search_request_id),
		constraint fk_search_request_container_container_id foreign key (container_id) references container(container_id)
	)
end


--"search_request_term" table holds all the search terms linked to a given request
if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'search_request_term')
begin
	create table search_request_term
	(
		search_request_id int not null,
		search_term_id int not null
	)
end

go

--------------- Install the base data required by Rummage ---------------------

if not exists (select 1 from container_type where code = 'DIR')
begin
	insert into container_type (container_type_id, code, name) values (1, 'DIR', 'Filesystem directory');
end

if not exists (select 1 from container_type where code = 'SQL')
begin
	insert into container_type (container_type_id, code, name) values (2, 'SQL', 'SQL table');
end

go

