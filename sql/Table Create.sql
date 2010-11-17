--DROP TABLE settings;

CREATE TABLE settings
(
    setting_id              integer NOT NULL,
    setting_type            char(3) not null,
    setting_value_int       integer,
    setting_value_float     float,
    setting_value_varchar   varchar(2000),
    setting_value_bool char(1),
    
    
    CONSTRAINT pk_setting_id
        PRIMARY KEY (setting_id),
    CONSTRAINT ck_setting_type
        CHECK       (setting_type in ('INT', 'FLT', 'CHR', 'BLN')),
    CONSTRAINT ck_setting_value_bool
        CHECK       (setting_value_bool in ('T', 'F'))
);

--Create the generator for the id field