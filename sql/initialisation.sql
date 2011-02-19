--*************************************************************************************************
--* System: Rummage                                         Type: Firebird SQL                    *
--* Author: Xanthalas                                       Date: October 2010                    *
--*                                                                                               *
--* This file contains the SQL required to create all the tables and basedata used by Rummage.    *
--*                                                                                               *
--*************************************************************************************************

--"container_type" table holds the different types of containers available to Rummage
create table container_type
(
    container_type_id int not null primary key,
    code char(3) not null,
    name varchar(20) not null
);

--"container" table holds all the containers which have been searched by Rummage
create table container (
    container_id int not null primary key,
    container_url varchar(8192),
    container_type_id int not null references container_type(container_type_id)
);

--"search_term" table holds all the search terms which Rummage has searched for
create table search_term
(
    search_term_id int not null primary key,
    search_term varchar(1024) not null
);

--"search_request" holds all the parameters from a single search request
create table search_request 
(
  search_request_id int not null primary key,
  search_request_guid varchar(64),
  name varchar(64),
  case_sensitive char(1),
  search_hidden char(1),
  search_binaries char(1),
  recurse char(1),
  date_request_created timestamp default current_timestamp,
  date_search_last_run timestamp default current_timestamp
);

--"search_request_container" holds all the containers searched for a request
create table search_request_container
(
    search_request_id int not null references search_request(search_request_id),
    container_id int not null references container(container_id)
);

--"search_request_term" table holds all the search terms linked to a given request
create table search_request_term
(
    search_request_id int not null,
    search_term_id int not null
);

--Create various indexes
create index ix_search_request_term on search_request_term (search_request_id);


commit

--------------- Install the base data required by Rummage ---------------------

insert into container_type (container_type_id, code, name) values (1, 'DIR', 'Filesystem directory');
insert into container_type (container_type_id, code, name) values (2, 'SQL', 'SQL table');

commit

