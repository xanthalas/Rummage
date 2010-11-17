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

--"containers" table holds all the containers which have been searched by Rummage
create table containers (
    container_id int not null primary key,
    container varchar(8192),
    container_type_id int not null references container_type(container_type_id)
);


--"search_terms" table holds all the search terms which Rummage has searched for
create table search_terms
(
    search_term_id int not null primary key,
    search_term varchar(1024) not null
);

--"search_request" holds all the parameters from a single search request
create table search_request 
(
  search_request_id int not null primary key,
  name varchar(64),
  date_request_created timestamp default current_timestamp,
  date_search_last_run timestamp default current_timestamp
);

create table search_request_containers
(
    search_request_id int not null references search_request(search_request_id),
    container_id int not null references containers(container_id)
);



--------------- Install the base data required by Rummage ---------------------

insert into container_type (container_type_id, code, name) values (1, 'DIR', 'Filesystem directory');
insert into container_type (container_type_id, code, name) values (2, 'SQL', 'SQL table');



