CREATE OR REPLACE FUNCTION sys_scheme.create_history_table(id_table_val integer)
  RETURNS integer AS
$BODY1$DECLARE
scheme_name_val character varying;
scheme_table_val character varying;
pk_field_val character varying;
exists_table_history boolean;
exists_seq boolean;
id_field_val integer;
field_info_item sys_scheme.table_field_info;
sql_string character varying;
id_history_table_val integer;
t_info RECORD;
BEGIN
    sql_string:='';
    exists_seq:=false;
    SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT name_db INTO scheme_table_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like scheme_name_val AND tablename like scheme_table_val||'_history') INTO exists_table_history;

    id_history_table_val:=nextval('sys_scheme.table_history_info_id_history_table_seq');

    IF exists_table_history=TRUE THEN
        EXECUTE 'ALTER TABLE '||scheme_name_val||'.'||scheme_table_val||'_history RENAME TO '||scheme_table_val||
            '_history'||id_history_table_val||';';
        UPDATE sys_scheme.table_history_info 
            SET history_table_name = scheme_table_val||'_history'||id_history_table_val 
            WHERE id_table = id_table_val AND history_table_name like scheme_table_val||'_history';
        INSERT INTO sys_scheme.table_history_info (id_table, dataupd, history_table_name, is_work, id_history_table)
            VALUES (id_table_val, now(), scheme_table_val||'_history', TRUE, id_history_table_val);
    ELSE
        INSERT INTO sys_scheme.table_history_info (id_table, dataupd, history_table_name, is_work, id_history_table)
            VALUES (id_table_val, now(), scheme_table_val||'_history', TRUE, id_history_table_val);
        EXECUTE 'CREATE OR REPLACE FUNCTION '||scheme_name_val||'.trg_after_'||scheme_table_val||'_history() RETURNS trigger AS '||
                --'DECLARE '||
                '$BODY$BEGIN '||
                'IF TG_OP=''INSERT'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||
                        '_history_id_history_seq'''||'), now() as dataupd, 1, current_user, new.*; '||
                'END IF; '||
                'IF TG_OP=''UPDATE'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val
                        ||'_history_id_history_seq'''||'), now() as dataupd, 2, current_user, new.*; '||
                'END IF; '||
                'RETURN new; '||
                'END;$BODY$ '||
                'LANGUAGE plpgsql VOLATILE '||
                'COST 100; '||
                'ALTER FUNCTION "'||scheme_name_val||'".trg_after_'||scheme_table_val||'_history()'||
		'OWNER TO '|| sys_scheme.get_admin_group() ||';';

        EXECUTE 'CREATE OR REPLACE FUNCTION '||scheme_name_val||'.trg_before_'||scheme_table_val||'_history() RETURNS trigger AS '||
                --'DECLARE '||
                '$BODY$BEGIN '||
                'IF TG_OP=''DELETE'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||
                        '_history_id_history_seq'''||'), now() as dataupd, 3, current_user, OLD.*; '||
                'END IF; '||
                'return OLD; '||
                'END;$BODY$ '||
                'LANGUAGE plpgsql VOLATILE '||
                'COST 100;'||
                'ALTER FUNCTION "'||scheme_name_val||'".trg_before_'||scheme_table_val||'_history()'||
		'OWNER TO '|| sys_scheme.get_admin_group() ||';';

        EXECUTE 'CREATE TRIGGER delete_'||scheme_table_val||'_history '||
                'BEFORE DELETE '||
                'ON '||scheme_name_val||'.'||scheme_table_val||' '||
                'FOR EACH ROW '||
                'EXECUTE PROCEDURE '||scheme_name_val||'.trg_before_'||scheme_table_val||'_history(); '||
            'CREATE TRIGGER insert_update_'||scheme_table_val||'_history '||
                'AFTER INSERT OR UPDATE '||
                'ON '||scheme_name_val||'.'||scheme_table_val||' '||
                'FOR EACH ROW '||
                'EXECUTE PROCEDURE '||scheme_name_val||'.trg_after_'||scheme_table_val||'_history();';
    END IF;
    
    sql_string:='CREATE TABLE '||scheme_name_val||'.'||scheme_table_val||'_history';
    IF exists_table_history = FALSE THEN
        sql_string:= sql_string||' ( id_history serial, ';
    ELSE
        sql_string:= sql_string||' ( id_history integer NOT NULL DEFAULT nextval('''||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq''::regclass), ';
    END IF;
            
    sql_string:= sql_string||' dataupd timestamp without time zone NOT NULL DEFAULT now(),'||
    'type_operation integer NOT NULL DEFAULT 1,'||
    'user_name character varying,';

    FOR t_info IN ( SELECT  t.table_name, c.column_name, c.data_type
               FROM information_schema.tables t JOIN information_schema.COLUMNS c ON t.table_name::text = c.table_name::text AND t.table_schema::text = c.table_schema::text
              WHERE t.table_schema::text = scheme_name_val::text AND 
                t.table_catalog::name = current_database() AND 
                t.table_type::text = 'BASE TABLE'::text AND 
                t.table_name = scheme_table_val
              ORDER BY t.table_name, c.ordinal_position) LOOP
        IF t_info.data_type='integer' THEN
            sql_string:=sql_string||
                t_info.column_name||' integer,';
        ELSIF t_info.data_type='bigint' THEN
            sql_string:=sql_string||
                t_info.column_name||' bigint,';
        ELSIF t_info.data_type='character varying' OR t_info.data_type='text' OR t_info.data_type='character' THEN
            sql_string:=sql_string||
                t_info.column_name||' character varying,';        
        ELSIF t_info.data_type='date' THEN
            sql_string:=sql_string||
                t_info.column_name||' date,';
        ELSIF t_info.data_type='timestamp without time zone' OR t_info.data_type='timestamp with time zone' THEN
            sql_string:=sql_string||
                t_info.column_name||' timestamp with time zone,';
        ELSIF t_info.data_type='USER-DEFINED' THEN
            sql_string:=sql_string||
                t_info.column_name||' geometry,';
        ELSIF t_info.data_type='numeric' OR t_info.data_type='double precision' THEN
            sql_string:=sql_string||
                t_info.column_name||' numeric,';
        ELSIF t_info.data_type='real' THEN
            sql_string:=sql_string||
                t_info.column_name||' real,';
        ELSIF t_info.data_type='boolean' THEN
            sql_string:=sql_string||
                t_info.column_name||' boolean,';
        END IF;
    END LOOP;

    sql_string:=sql_string||' CONSTRAINT '||scheme_table_val||'_history'||id_history_table_val||'_pkey PRIMARY KEY (id_history));';
    
    sql_string:=sql_string||' GRANT ALL ON TABLE '||scheme_name_val||'.'||scheme_table_val||'_history TO public;';
    sql_string:=sql_string||' ALTER TABLE '||scheme_name_val||'.'||scheme_table_val||'_history OWNER TO '|| sys_scheme.get_admin_group() ||';';
    EXECUTE sql_string;
    raise notice 'SQL:: %', sql_string;
    IF exists_table_history=FALSE THEN
        PERFORM sys_scheme.set_photo_history(id_table_val);
    END IF;
    --EXECUTE 'ALTER TABLE "'||scheme_name_val||'"."'||scheme_table_val||'" ENABLE TRIGGER ALL';

    IF exists_table_history=FALSE THEN
        SELECT EXISTS(SELECT 1 FROM information_schema.sequences WHERE sequence_schema like scheme_name_val 
                    AND sequence_name like scheme_table_val||'_history_id_history_seq') INTO exists_seq;
        IF exists_seq = FALSE THEN
            EXECUTE 'CREATE SEQUENCE '||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq
                  INCREMENT 1
                  MINVALUE 1
                  MAXVALUE 9223372036854775807
                  START 1
                  CACHE 1;';
        END IF;
        EXECUTE 'GRANT ALL ON TABLE '||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq TO public;';
        EXECUTE 'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||
            '_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq'''||'), now() as dataupd, 1, current_user, t.* FROM '||scheme_name_val||'.'||scheme_table_val||' t';
        
    END IF;
RETURN id_history_table_val;
END;$BODY1$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sys_scheme.create_history_table(integer)
  OWNER TO postgres;
COMMENT ON FUNCTION sys_scheme.create_history_table(integer) IS 'Создание табилцы истории';

CREATE OR REPLACE FUNCTION sys_scheme.create_history_table(id_table_val integer, create_photo_history boolean)
  RETURNS integer AS
$BODY1$DECLARE
scheme_name_val character varying;
scheme_table_val character varying;
pk_field_val character varying;
exists_table_history boolean;
exists_seq boolean;
id_field_val integer;
field_info_item sys_scheme.table_field_info;
sql_string character varying;
id_history_table_val integer;
t_info RECORD;
BEGIN
    sql_string:='';
    exists_seq:=false;
    SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT name_db INTO scheme_table_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
    SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like scheme_name_val AND tablename like scheme_table_val||'_history') INTO exists_table_history;

    id_history_table_val:=nextval('sys_scheme.table_history_info_id_history_table_seq');

    IF exists_table_history=TRUE THEN
        EXECUTE 'ALTER TABLE '||scheme_name_val||'.'||scheme_table_val||'_history RENAME TO '||scheme_table_val||
            '_history'||id_history_table_val||';';
        UPDATE sys_scheme.table_history_info 
            SET history_table_name = scheme_table_val||'_history'||id_history_table_val 
            WHERE id_table = id_table_val AND history_table_name like scheme_table_val||'_history';
        INSERT INTO sys_scheme.table_history_info (id_table, dataupd, history_table_name, is_work, id_history_table)
            VALUES (id_table_val, now(), scheme_table_val||'_history', TRUE, id_history_table_val);
    ELSE
        INSERT INTO sys_scheme.table_history_info (id_table, dataupd, history_table_name, is_work, id_history_table)
            VALUES (id_table_val, now(), scheme_table_val||'_history', TRUE, id_history_table_val);
        EXECUTE 'CREATE OR REPLACE FUNCTION '||scheme_name_val||'.trg_after_'||scheme_table_val||'_history() RETURNS trigger AS '||
                --'DECLARE '||
                '$BODY$BEGIN '||
                'IF TG_OP=''INSERT'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||
                        '_history_id_history_seq'''||'), now() as dataupd, 1, current_user, new.*; '||
                'END IF; '||
                'IF TG_OP=''UPDATE'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val
                        ||'_history_id_history_seq'''||'), now() as dataupd, 2, current_user, new.*; '||
                'END IF; '||
                'RETURN new; '||
                'END;$BODY$ '||
                'LANGUAGE plpgsql VOLATILE '||
                'COST 100; ';
        EXECUTE 'CREATE OR REPLACE FUNCTION '||scheme_name_val||'.trg_before_'||scheme_table_val||'_history() RETURNS trigger AS '||
                --'DECLARE '||
                '$BODY$BEGIN '||
                'IF TG_OP=''DELETE'' THEN '||
                'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||'_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||
                        '_history_id_history_seq'''||'), now() as dataupd, 3, current_user, OLD.*; '||
                'END IF; '||
                'return OLD; '||
                'END;$BODY$ '||
                'LANGUAGE plpgsql VOLATILE '||
                'COST 100;';
        EXECUTE 'CREATE TRIGGER delete_'||scheme_table_val||'_history '||
                'BEFORE DELETE '||
                'ON '||scheme_name_val||'.'||scheme_table_val||' '||
                'FOR EACH ROW '||
                'EXECUTE PROCEDURE '||scheme_name_val||'.trg_before_'||scheme_table_val||'_history(); '||
            'CREATE TRIGGER insert_update_'||scheme_table_val||'_history '||
                'AFTER INSERT OR UPDATE '||
                'ON '||scheme_name_val||'.'||scheme_table_val||' '||
                'FOR EACH ROW '||
                'EXECUTE PROCEDURE '||scheme_name_val||'.trg_after_'||scheme_table_val||'_history();';
    END IF;
    
    sql_string:='CREATE TABLE '||scheme_name_val||'.'||scheme_table_val||'_history';
    IF exists_table_history = FALSE THEN
        sql_string:= sql_string||' ( id_history serial, ';
    ELSE
        sql_string:= sql_string||' ( id_history integer NOT NULL DEFAULT nextval('''||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq''::regclass), ';
    END IF;
            
    sql_string:= sql_string||' dataupd timestamp without time zone NOT NULL DEFAULT now(),'||
    'type_operation integer NOT NULL DEFAULT 1,'||
    'user_name character varying,';

    FOR t_info IN ( SELECT  t.table_name, c.column_name, c.data_type
               FROM information_schema.tables t JOIN information_schema.COLUMNS c ON t.table_name::text = c.table_name::text AND t.table_schema::text = c.table_schema::text
              WHERE t.table_schema::text = scheme_name_val::text AND 
                t.table_catalog::name = current_database() AND 
                t.table_type::text = 'BASE TABLE'::text AND 
                t.table_name = scheme_table_val
              ORDER BY t.table_name, c.ordinal_position) LOOP
        IF t_info.data_type='integer' THEN
            sql_string:=sql_string||
                t_info.column_name||' integer,';
        ELSIF t_info.data_type='bigint' THEN
            sql_string:=sql_string||
                t_info.column_name||' bigint,';
        ELSIF t_info.data_type='character varying' OR t_info.data_type='text' OR t_info.data_type='character' THEN
            sql_string:=sql_string||
                t_info.column_name||' character varying,';        
        ELSIF t_info.data_type='date' THEN
            sql_string:=sql_string||
                t_info.column_name||' date,';
        ELSIF t_info.data_type='timestamp without time zone' OR t_info.data_type='timestamp with time zone' THEN
            sql_string:=sql_string||
                t_info.column_name||' timestamp with time zone,';
        ELSIF t_info.data_type='USER-DEFINED' THEN
            sql_string:=sql_string||
                t_info.column_name||' geometry,';
        ELSIF t_info.data_type='numeric' OR t_info.data_type='double precision' THEN
            sql_string:=sql_string||
                t_info.column_name||' numeric,';
        ELSIF t_info.data_type='real' THEN
            sql_string:=sql_string||
                t_info.column_name||' real,';
        ELSIF t_info.data_type='boolean' THEN
            sql_string:=sql_string||
                t_info.column_name||' boolean,';
        END IF;
    END LOOP;

    sql_string:=sql_string||' CONSTRAINT '||scheme_table_val||'_history'||id_history_table_val||'_pkey PRIMARY KEY (id_history));';
    
    sql_string:=sql_string||' GRANT ALL ON TABLE '||scheme_name_val||'.'||scheme_table_val||'_history TO public;';
    EXECUTE sql_string;
    raise notice 'SQL:: %', sql_string;
    IF exists_table_history=FALSE AND create_photo_history=TRUE THEN
        PERFORM sys_scheme.set_photo_history(id_table_val);
    END IF;
    --EXECUTE 'ALTER TABLE "'||scheme_name_val||'"."'||scheme_table_val||'" ENABLE TRIGGER ALL';

    IF exists_table_history=FALSE THEN
        SELECT EXISTS(SELECT 1 FROM information_schema.sequences WHERE sequence_schema like scheme_name_val 
                    AND sequence_name like scheme_table_val||'_history_id_history_seq') INTO exists_seq;
        IF exists_seq = FALSE THEN
            EXECUTE 'CREATE SEQUENCE '||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq
                  INCREMENT 1
                  MINVALUE 1
                  MAXVALUE 9223372036854775807
                  START 1
                  CACHE 1;';
        END IF;
        EXECUTE 'GRANT ALL ON TABLE '||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq TO public;';
        EXECUTE 'INSERT INTO '||scheme_name_val||'.'||scheme_table_val||
            '_history SELECT nextval('''||scheme_name_val||'.'||scheme_table_val||'_history_id_history_seq'''||'), now() as dataupd, 1, current_user, t.* FROM '||scheme_name_val||'.'||scheme_table_val||' t';
        
    END IF;
RETURN id_history_table_val;
END;$BODY1$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  