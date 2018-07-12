CREATE OR REPLACE FUNCTION sys_scheme.set_right(table_id integer, user_id integer, read_val boolean, write_val boolean)
  RETURNS boolean AS
$BODY$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_right WHERE id_table = table_id AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.table_right SET read_data = read_val, write_data = write_val WHERE id_table = table_id AND id_user = user_id;
	ELSE
		INSERT INTO sys_scheme.table_right (id_table, id_user, read_data, write_data) VALUES (table_id, user_id, read_val, write_val);
	END IF;
		IF read_val=true THEN
			PERFORM sys_scheme.set_read_right(table_id, user_id, true);
		END IF;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.create_exists_table_get_info(scheme_name_db character varying, table_name_db character varying, table_name_map character varying, pk_field character varying, geom_field_f character varying)
  RETURNS integer AS
$BODY$DECLARE
id_table_new integer;
r information_schema.columns;
geomtype character varying;
pk_field_temp character varying;
geomtypeint integer;
BEGIN
--column_name, data_type, udt_name
SELECT "type" INTO geomtype FROM geometry_columns WHERE f_table_schema like scheme_name_db AND f_table_name like table_name_db AND f_geometry_column like geom_field_f;
SELECT id INTO geomtypeint FROM sys_scheme.table_type_geom WHERE (namedb like '%'||replace(geomtype, 'MULTI', '')||'%') LIMIT 1; 

IF geomtypeint is NULL THEN
	RETURN -1;
END IF;
IF pk_field is NULL OR pk_field = '' THEN
	SELECT ccu.column_name INTO pk_field_temp FROM information_schema.constraint_column_usage ccu, information_schema.table_constraints tc 
		WHERE (tc.constraint_schema like scheme_name_db) 
		AND (tc.table_name like table_name_db) 
		AND (tc.constraint_type like 'PRIMARY KEY')
		AND (ccu.constraint_name = tc.constraint_name);
ELSE
	pk_field_temp:=pk_field;
END IF;
IF pk_field_temp is NULL OR pk_field_temp = '' THEN
	RETURN -2;
END IF;
id_table_new:= sys_scheme.create_exists_table(scheme_name_db, table_name_db, table_name_map, pk_field_temp, 1, geomtypeint, false, false, 0);
--raise notice '????? %', '1';
IF id_table_new>0 THEN
--raise notice '????? %', '2';
	UPDATE sys_scheme.table_info SET geom_field = geom_field_f WHERE id = id_table_new;
	FOR r IN SELECT * FROM information_schema.columns 
		WHERE table_schema like scheme_name_db AND table_name like table_name_db
/*		AND
		(data_type like 'integer%' OR data_type like 'character varying' OR data_type like 'numeric' OR data_type like 'USER-DEFINED'  OR data_type like 'date') */
			/*AND (column_name not like pk_field_temp AND column_name not like geom_field_f)*/ LOOP
		IF r.data_type = 'integer' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 1, r.column_name);
		--END IF;
		ELSIF r.data_type = 'character varying' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 2, r.column_name);
		--END IF;
		ELSIF r.data_type = 'numeric' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 6, r.column_name);
		ELSIF r.data_type = 'double precision' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 6, r.column_name);
		ELSIF r.data_type = 'USER-DEFINED' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 5, r.column_name);
		ELSIF r.data_type = 'date' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 3, r.column_name);
		END IF;
		--raise notice '????? %', r.column_name;
	END LOOP;
END IF;

RETURN id_table_new;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.create_instead_trigger_for_view(id_table_val integer)
  RETURNS text AS
$BODY$DECLARE
	__table_name text;
	__table_scheme text;
	__view_name text;
	__pk_field text;
	__name_db text;
	__sql_create_trigger text;
	__insert_set text = '';
	__insert_value text = '';
	__rec_field RECORD;
	__ref_type integer;
BEGIN

	SELECT 	name_db, scheme_name, view_name, pk_fileld 
	INTO 	__table_name, __table_scheme, __view_name, __pk_field 
	FROM 	sys_scheme.table_info 
	WHERE 	id = id_table_val;

	FOR 	__rec_field 
	IN 
		SELECT "name_db", "type_field", "is_reference", "is_interval", "ref_table", "ref_field", "ref_field_end", "ref_field_name"
		FROM sys_scheme.table_field_info
		WHERE "id_table" = id_table_val AND "visible" = TRUE ORDER BY "num_order" 
	LOOP
		IF (__rec_field.is_reference = TRUE  AND __rec_field.is_interval = FALSE)
		THEN
			__ref_type := 1;
		ELSIF (__rec_field.is_reference = FALSE  AND __rec_field.is_interval = TRUE)
		THEN
			__ref_type := 2;
		ELSE
			__ref_type := 0;
		END IF;
		IF 	(__ref_type = 0)
		THEN
			IF 	(__insert_value != '')
			THEN
				__insert_value = __insert_value || ',';
				__insert_set = __insert_set || ',';
			END IF;
			
			__insert_set := __insert_set || ' "' || __rec_field.name_db || '"';
			__insert_value := __insert_value || ' NEW."' || __rec_field.name_db || '"';
		END IF;
	END LOOP;
	
	__sql_create_trigger:=
'CREATE OR REPLACE FUNCTION "'||__table_scheme||'"."'||__view_name||'_instead"()
RETURNS trigger AS
$BODY_FUNCTION$
BEGIN

IF TG_OP = ''INSERT'' THEN
	INSERT INTO "'||__table_scheme||'"."'||__table_name||'" (' || __insert_set || ') VALUES (' || __insert_value || ');
	RETURN NEW;
ELSIF TG_OP = ''UPDATE'' THEN
	UPDATE "'||__table_scheme||'"."'||__table_name||'"  SET (' || __insert_set || ') = (' || __insert_value || ') WHERE "'||__pk_field||'" = OLD."'||__pk_field||'";
	RETURN NEW;
ELSIF TG_OP = ''DELETE'' THEN
	DELETE FROM "'||__table_scheme||'"."'||__table_name||'" WHERE "'||__pk_field||'" = OLD."'||__pk_field||'";
	RETURN NULL;
END IF;
RETURN NEW;
END;
$BODY_FUNCTION$
LANGUAGE plpgsql VOLATILE
COST 100;

ALTER FUNCTION "'||__table_scheme||'"."'||__view_name||'_instead"() OWNER TO ' || sys_scheme.get_admin_group() ||';
	
DROP TRIGGER IF EXISTS "'||__view_name||'_trg" ON "'||__table_scheme||'"."'||__view_name||'";

CREATE TRIGGER "'||__view_name||'_trg"
  INSTEAD OF INSERT OR UPDATE OR DELETE
  ON "'||__table_scheme||'"."'||__view_name||'"
  FOR EACH ROW
  EXECUTE PROCEDURE "'||__table_scheme||'"."'||__view_name||'_instead"();';

	RETURN __sql_create_trigger;
	
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.create_view_for_table(id_table_val integer)
  RETURNS character varying AS
$BODY$DECLARE
table_name_val character varying;
table_scheme_val character varying;
table_type_val integer;
table_geom_val character varying;
exists_in_gc boolean;
BEGIN

	SELECT name_db, scheme_name INTO table_name_val, table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;

	EXECUTE 'DROP VIEW IF EXISTS "'||table_scheme_val||'"."'||table_name_val||'_vw"; CREATE OR REPLACE VIEW "'||
	table_scheme_val||'"."'||table_name_val||'_vw" AS '||sys_scheme.get_sql_create_view(id_table_val)||';
	GRANT ALL PRIVILEGES ON "'||table_scheme_val||'"."'||table_name_val||'_vw" TO public;
	ALTER TABLE "'||table_scheme_val||'"."'||table_name_val||'_vw" OWNER TO ' || sys_scheme.get_admin_group() ||';';
	UPDATE sys_scheme.table_info SET view_name = table_name_val||'_vw', sql_view_string = sys_scheme.get_sql_for_view(id_table_val) WHERE id = id_table_val;
	SELECT "type" INTO table_type_val FROM sys_scheme.table_info WHERE id = id_table_val;
	IF table_type_val=1 THEN
		SELECT geom_field INTO table_geom_val FROM sys_scheme.table_info WHERE id = id_table_val;
		SELECT EXISTS(SELECT true FROM geometry_columns WHERE f_table_schema like table_scheme_val 
			AND f_table_name like table_name_val||'_vw' 
			AND f_geometry_column like table_geom_val) INTO exists_in_gc;
		IF exists_in_gc=FALSE THEN
			INSERT INTO geometry_columns(f_table_catalog, f_table_schema, f_table_name, f_geometry_column, 
				 	coord_dimension, srid, "type")
			SELECT gc.f_table_catalog, table_scheme_val, table_name_val||'_vw', table_geom_val, 2, gc.srid, gc."type"
			FROM geometry_columns gc
			WHERE  gc.f_table_schema like table_scheme_val 
			AND gc.f_table_name like table_name_val 
			AND gc.f_geometry_column like table_geom_val;
		END IF;
	END IF;	
	EXECUTE sys_scheme.create_instead_trigger_for_view(id_table_val);
RETURN table_name_val||'_vw';
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.delete_field(field_id integer, real_delete boolean)
  RETURNS boolean AS
$BODY$DECLARE
table_shemename character varying;
table_namedb character varying;
field_namedb character varying;
table_id integer;
exists_history boolean;
BEGIN
	SELECT id_table INTO table_id FROM sys_scheme.table_field_info WHERE id = field_id;
	SELECT name_db INTO field_namedb FROM sys_scheme.table_field_info WHERE id = field_id;
	SELECT scheme_name INTO table_shemename FROM sys_scheme.table_info WHERE id = table_id;
	SELECT name_db INTO table_namedb FROM sys_scheme.table_info WHERE id = table_id;

	DELETE FROM sys_scheme.table_field_info WHERE id = field_id;

	IF real_delete=TRUE THEN
		EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||' DROP COLUMN '||field_namedb||' CASCADE;';
		EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	END IF;

	RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.del_field()
  RETURNS trigger AS
$BODY$DECLARE
cnt integer;
exists_history boolean;
exists_table boolean;
BEGIN
	IF (SELECT style_field FROM sys_scheme.table_info WHERE id = OLD.id_table) = OLD.name_db
	THEN UPDATE sys_scheme.table_info SET default_style = TRUE, style_field = 'style' WHERE id = OLD.id_table;
	END IF;
	IF (SELECT range_column FROM sys_scheme.table_info WHERE id = OLD.id_table) = OLD.name_db
	THEN UPDATE sys_scheme.table_info SET range_column = NULL, range_colors = FALSE WHERE id = OLD.id_table;
	END IF;
	IF (SELECT lablefiled FROM sys_scheme.table_info WHERE id = OLD.id_table) like '%(('||OLD.name_db||')::text)%' 
	THEN UPDATE sys_scheme.table_info SET label_showlabel = FALSE, lablefiled = replace(lablefiled, '(('||OLD.name_db||')::text)', '(('''||OLD.name_db||''')::text)')  WHERE id = OLD.id_table;
	END IF;

	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_info WHERE id = OLD.id_table) INTO exists_table;
	IF exists_table=TRUE THEN
		PERFORM sys_scheme.create_view_for_table(OLD.id_table);
	END IF;

	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = OLD.id_table) INTO exists_history;

	IF exists_history=TRUE THEN
		PERFORM sys_scheme.create_history_table(OLD.id_table);
	END IF;

	RETURN NEW;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.create_table_with_group_name(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group_name character varying, def_visible boolean, is_hidden boolean, srid integer)
  RETURNS integer AS
$BODY$DECLARE
id_new_table integer;
group_id_val integer;
BEGIN
SELECT sys_scheme.create_table(table_scheme_name, table_name_db,
table_name_sys, table_type, table_geom_type, srid, is_style, photo_exists,
null, def_visible, is_hidden) INTO id_new_table;
IF id_new_table>=0 THEN
       IF table_group_name IS NOT NULL THEN
               SELECT sys_scheme.add_group(table_group_name, table_group_name) INTO group_id_val;
               INSERT INTO sys_scheme.table_groups_table (id_table, id_group) VALUES (id_new_table, group_id_val);
       END IF;
END IF;
RETURN id_new_table;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  

CREATE OR REPLACE FUNCTION sys_scheme.set_user_param(param_name text, user_id integer, param_ids text[])
  RETURNS boolean AS
$BODY$DECLARE
user_exists boolean;
exists_item boolean;
BEGIN
	SELECT EXISTS( SELECT 1 FROM sys_scheme.user_db WHERE id = user_id) INTO user_exists; 
	IF user_exists = false THEN
		RETURN false; 
	END IF; 
	SELECT EXISTS(SELECT 1 FROM sys_scheme.user_params WHERE name = param_name AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.user_params SET values = param_ids WHERE name = param_name AND id_user = user_id;
	ELSE
		INSERT INTO sys_scheme.user_params (name, id_user, values) VALUES (param_name, user_id, param_ids);
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.change_user_type(user_id integer, user_type integer)
  RETURNS boolean AS
$BODY$DECLARE
current_type integer;
user_login text;
BEGIN
	SELECT typ, "login" INTO current_type, user_login FROM sys_scheme.user_db WHERE id = $1;
	IF user_login IS NULL THEN
		RETURN FALSE;
	END IF;
	IF current_type=1 THEN
		IF $2=1 THEN
			RETURN FALSE;
		END IF;
		IF $2=2 THEN
			EXECUTE 'REVOKE '||sys_scheme.get_admin_group()||' FROM '||user_login||';';
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO '||user_login||';';
			EXECUTE 'ALTER ROLE '||user_login||' NOCREATEDB NOCREATEROLE;';
			UPDATE sys_scheme.user_db SET typ=2, "admin" = FALSE WHERE id = $1;
			RETURN TRUE;
		END IF;
	ELSE
		IF $2=1 THEN
			EXECUTE 'GRANT '||sys_scheme.get_admin_group()||' TO '||user_login||';';
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO '||user_login||';';
			EXECUTE 'ALTER ROLE '||user_login||' CREATEDB CREATEROLE;';
			UPDATE sys_scheme.user_db SET typ=1, "admin" = TRUE WHERE id = $1;
			RETURN TRUE;
		END IF;
		IF $2=2 THEN
			RETURN FALSE;
		END IF;
	END IF;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
CREATE OR REPLACE FUNCTION sys_scheme.set_read_right(table_id integer, user_id integer, read_val boolean)
  RETURNS boolean AS
$BODY$DECLARE
exists_item boolean;
linked_id INTEGER;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_right WHERE id_table = table_id AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.table_right SET read_data = read_val WHERE id_table = table_id AND id_user = user_id;
	ELSE
		
		INSERT INTO sys_scheme.table_right (id_table, id_user, read_data) 
		VALUES (table_id, user_id, read_val);
raise notice 'Право чтения таблице: %', table_id;

	END IF;
	IF read_val = true THEN
		FOR linked_id IN (SELECT ref_table
			FROM sys_scheme.table_field_info 
			WHERE id_table=table_id AND ref_table IS NOT NULL)
		LOOP
			PERFORM sys_scheme.set_read_right(linked_id, user_id, read_val);
		END LOOP;
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
COMMENT ON FUNCTION sys_scheme.set_read_right(integer, integer, boolean) IS 'Назначение права чтения на таблицу Инфраструктуры';

CREATE OR REPLACE FUNCTION sys_scheme.create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, srid integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean, is_hidden boolean)
  RETURNS integer AS
$BODY$DECLARE
exists_photo boolean;
exists_table boolean;
type_geom_name character varying;
id_new_table integer;
BEGIN

SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like table_scheme_name AND tablename like 'photo_'||table_name_db) INTO exists_photo;
SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like table_scheme_name AND tablename like table_name_db) INTO exists_table;

IF table_geom_type<>0 THEN
SELECT namedb INTO type_geom_name FROM sys_scheme.table_type_geom WHERE id = table_geom_type;
END IF;

IF exists_table=false THEN
	EXECUTE 'CREATE TABLE '||table_scheme_name||'.'||table_name_db||' (gid serial NOT NULL , PRIMARY KEY (gid)) WITH (OIDS = FALSE);';
ELSE
	RETURN -1;
END IF;

IF table_type = 1 THEN
	EXECUTE 'SELECT AddGeometryColumn('''|| table_scheme_name ||''', '''||table_name_db ||''',''the_geom'','||srid||','''|| type_geom_name ||''',2);';
	EXECUTE 'ALTER TABLE '||table_scheme_name||'.'||table_name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	PERFORM sys_scheme.create_index(table_scheme_name, table_name_db, 'the_geom');
END IF;

SELECT nextval('sys_scheme.table_info_id_seq')::integer INTO id_new_table;

IF photo_exists = TRUE THEN
	IF exists_photo = FALSE THEN
		EXECUTE 'CREATE TABLE '||table_scheme_name||'.photo_'||table_name_db||'(id serial NOT NULL, '||
                                      'id_obj integer, '||
                                      'file bytea NOT NULL, '||
                                      'img_preview bytea, ' ||
                                      'file_name character varying, '||
                                      'is_photo boolean, ' ||
                                      'dataupd timestamp without time zone DEFAULT now(), '||
                                      'master_id integer, '||
				      'status integer, '||
                                      'CONSTRAINT photo_'||table_name_db||'_pkey PRIMARY KEY ('||'id'||'), '||
                                      ' CONSTRAINT photo_'||table_name_db ||'_id_obj_fkey FOREIGN KEY (id_obj) '||
                                      ' REFERENCES '|| table_scheme_name ||'.'|| table_name_db ||' (gid) MATCH SIMPLE '||
                                      ' ON UPDATE CASCADE ON DELETE CASCADE '||
                                    ') '||
                                    'WITH ('||
                                      'OIDS=FALSE'||
                                    ');'||
                                    'GRANT ALL ON TABLE '|| table_scheme_name ||'.photo_'|| table_name_db ||' TO public; '||
                                    'ALTER TABLE '|| table_scheme_name ||'.photo_'|| table_name_db ||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	END IF;
END IF;

INSERT INTO sys_scheme.table_info (id, name_db, name_map, "geom_type", "type", map_style, "scheme_name", "photo",default_visibl ,hidden, 
				geom_field, pk_fileld)
	VALUES (id_new_table, table_name_db, table_name_sys, table_geom_type, table_type, is_style, table_scheme_name, 
	photo_exists,def_visible,is_hidden, 'the_geom', 'gid');

IF photo_exists = TRUE THEN
INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)
                                        VALUES (id_new_table, 'photo_'||table_name_db, 'id_obj', 'file', 'id');
END IF;

INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval, num_order) 
			VALUES (id_new_table, 'gid', 'gid', 1, true, 'Номер', false, false, 1);
IF table_type = 1 THEN
	INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval, num_order)
                   VALUES (id_new_table, 'the_geom', 'the_geom', 5, true, 'the_geom', false, false, 2);
END IF;

IF is_style=TRUE THEN
	EXECUTE 'ALTER TABLE '||table_scheme_name||'.'|| table_name_db||' '||
                                                            'ADD exists_style boolean DEFAULT true,'||
                                                            'ADD fontname character varying DEFAULT ''Map Symbols'','||
                                                            'ADD fontcolor integer DEFAULT 16711680,'||
                                                            'ADD fontframecolor integer DEFAULT 16711680,'||
                                                            'ADD fontsize integer DEFAULT 12,'||
                                                            'ADD symbol integer DEFAULT 35,'||
                                                            'ADD pencolor integer DEFAULT 16711680,'||
                                                            'ADD pentype integer DEFAULT 2,'||
                                                            'ADD penwidth integer DEFAULT 1,'||
                                                            'ADD brushbgcolor bigint DEFAULT 16711680,'||
                                                            'ADD brushfgcolor integer DEFAULT 16711680,'||
                                                            'ADD brushstyle integer DEFAULT 0,'||
                                                            'ADD brushhatch integer DEFAULT 1;'||
                                                            'ALTER TABLE '||table_scheme_name||'.'|| table_name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
END IF;
PERFORM sys_scheme.set_table_rights_for_admins(id_new_table);
IF table_group IS NOT NULL THEN
	INSERT INTO sys_scheme.table_groups_table (id_table, id_group) VALUES (id_new_table, table_group);
END IF;
RETURN id_new_table;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;





-- Function: sys_scheme.create_table(character varying, character varying, character varying, integer, integer, integer, boolean, boolean, integer)

-- DROP FUNCTION sys_scheme.create_table(character varying, character varying, character varying, integer, integer, integer, boolean, boolean, integer);

CREATE OR REPLACE FUNCTION sys_scheme.create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, srid integer, is_style boolean, photo_exists boolean, table_group integer)
  RETURNS integer AS
$BODY$BEGIN
	RETURN sys_scheme.create_table(table_scheme_name, table_name_db, table_name_sys, table_type , table_geom_type , srid, is_style , photo_exists, table_group, false, false);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;




-- Function: sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer)

-- DROP FUNCTION sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer);

CREATE OR REPLACE FUNCTION sys_scheme.create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer)
  RETURNS integer AS
$BODY$BEGIN
	RETURN sys_scheme.create_table(table_scheme_name, table_name_db, table_name_sys, table_type , table_geom_type , 4326, is_style , photo_exists, table_group, false, false);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
COMMENT ON FUNCTION sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer) IS 'Создание новой таблицы';



-- Function: sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer, boolean)

-- DROP FUNCTION sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer, boolean);

CREATE OR REPLACE FUNCTION sys_scheme.create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean)
  RETURNS integer AS
$BODY$BEGIN
RETURN sys_scheme.create_table(table_scheme_name, table_name_db, table_name_sys, table_type , table_geom_type , 4326, is_style , photo_exists, table_group, def_visible, false);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
COMMENT ON FUNCTION sys_scheme.create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer, boolean) IS 'Создание новой таблицы';


CREATE OR REPLACE FUNCTION sys_scheme.user_grant_clear(user_name text)
  RETURNS boolean AS
$BODY$DECLARE
rec RECORD;
BEGIN
	FOR rec in (SELECT n.nspname, c.relname, c.relkind 
			FROM pg_shdepend d, pg_user u, pg_class c, pg_database db, pg_namespace n
			WHERE n.oid = c.relnamespace AND c.oid = d.objid AND d.refobjid= u.usesysid AND u.usename = user_name
				AND d.dbid = db.oid AND db.datname = current_database()) LOOP
		IF rec.relkind = 'r' OR rec.relkind = 't' THEN
			EXECUTE 'REVOKE ALL ON TABLE "'||rec.nspname||'"."'||rec.relname||'" FROM "'||user_name||'"';
		ELSIF rec.relkind = 'S' THEN
			EXECUTE 'REVOKE ALL ON TABLE "'||rec.nspname||'"."'||rec.relname||'" FROM "'||user_name||'"';
		END IF;		
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.delete_user()
  RETURNS trigger AS
$BODY$DECLARE
db_name character varying;
login_user character varying; 
pass_user character varying;
rec RECORD;
BEGIN
	--SELECT current_database() into db_name;
	UPDATE sys_scheme.table_right SET read_data = FALSE, write_data = FALSE WHERE id_user=OLD.id;
	login_user = OLD."login";
	EXECUTE 'REVOKE '||sys_scheme.get_client_group()||' FROM '||login_user||';';
	EXECUTE 'REVOKE '||sys_scheme.get_admin_group()||' FROM '||login_user||';';
	PERFORM  sys_scheme.user_grant_clear(login_user);
	EXECUTE 'DROP USER '|| login_user;
	RETURN OLD;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
CREATE OR REPLACE FUNCTION sys_scheme.create_client_group()
  RETURNS boolean AS
$BODY$DECLARE
id_new_user integer;
exists_group boolean;
client_group_name character varying;
BEGIN
	client_group_name := sys_scheme.get_client_group();
	SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like client_group_name) INTO exists_group;
	IF exists_group=FALSE THEN
		EXECUTE 'CREATE ROLE ' || client_group_name||'
			NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE;
			COMMENT ON ROLE ' || client_group_name ||' IS ''Группа для простых пользователей'';';
	END IF;
	
	-- Выделений прав группе для пользователей
	EXECUTE 'GRANT CONNECT ON DATABASE ' || current_database() || ' TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON SCHEMA sys_scheme TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme."action" TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.actions_users TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_groups TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_groups_table TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_history_info TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_photo TO ' || client_group_name;
	EXECUTE 'GRANT all ON sys_scheme.journal TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.status TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_field_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_filtr_field_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_filtr_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_photo_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_right TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_style_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.user_db TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme."version" TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.typ_version TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.typ_users TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type_table TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type_geom TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_filtr_tip_operator TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_schems TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."journal_id_seq" TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON SCHEMA public TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON public.geometry_columns TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON public.spatial_ref_sys TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_history_info TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_history_info_id_history_table_seq TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_history_photo TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_history_photo_id_seq TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.tokens TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_files TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_journal TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_updater TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_user_files TO ' || client_group_name;

	EXECUTE 'GRANT SELECT ON sys_scheme.db_version TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.fts_tables TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.fts_fields TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.fts_index TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.report_templates TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.map_extents TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.user_params TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.sets_styles TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_info_sets TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_order_set TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_info_id_seq TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.sets_styles_id_seq TO ' || client_group_name;


	
	RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
CREATE OR REPLACE FUNCTION sys_scheme.create_admin_group()
  RETURNS boolean AS
$BODY$DECLARE
id_new_user integer;
exists_group boolean;
admin_group_name character varying;
BEGIN
	admin_group_name := sys_scheme.get_admin_group();
	SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like admin_group_name) INTO exists_group;
	IF exists_group=FALSE THEN
		EXECUTE 'CREATE ROLE ' || admin_group_name||'
			NOSUPERUSER INHERIT CREATEDB CREATEROLE;
			COMMENT ON ROLE ' || admin_group_name||' IS ''Группа для администраторов'';';
	END IF;
	-- Выделений прав группе для пользователей
	EXECUTE 'GRANT CONNECT ON DATABASE ' || current_database() || ' TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON SCHEMA sys_scheme TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme."action" TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.actions_users TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.journal TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.status TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_field_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_filtr_field_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_filtr_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_photo_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_right TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_style_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.user_db TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_history_info TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_history_photo TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_groups TO ' || admin_group_name;
	EXECUTE 'GRANT all ON sys_scheme.table_groups_table TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme."version" TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.typ_version TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.typ_users TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type_table TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type_geom TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_type TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_filtr_tip_operator TO ' || admin_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_schems TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."action_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."actions_users_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."journal_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_field_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_filtr_field_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_filtr_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_photo_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_right_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_style_info_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."user_db_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme."table_groups_id_seq" TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON SCHEMA public TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON public.geometry_columns TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON public.spatial_ref_sys TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_info TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_info_id_history_table_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_photo TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_photo_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.tokens TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.upd_files TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.upd_journal TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.upd_updater TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.upd_user_files TO ' || admin_group_name;

	EXECUTE 'GRANT ALL ON sys_scheme.db_version TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.fts_tables TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.fts_fields TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.fts_index TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.report_templates TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.map_extents TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.map_extents_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.fts_fields_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.report_templates_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.user_params TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.sets_styles TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_info_sets TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_order_set TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_info_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.sets_styles_id_seq TO ' || admin_group_name;
	
	RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
COMMENT ON FUNCTION sys_scheme.create_admin_group() IS 'Создание группы администраторов';

CREATE OR REPLACE FUNCTION sys_scheme.fix_table_owner()
  RETURNS boolean AS
$BODY$DECLARE
rec RECORD;
rec2 RECORD;
seq_name text;
exists_table_val boolean;
BEGIN
	FOR rec IN SELECT id, scheme_name, name_db, pk_fileld, view_name FROM sys_scheme.table_info LOOP
		SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like rec.name_db
						AND table_type like 'BASE TABLE') INTO exists_table_val;
		IF exists_table_val=true THEN
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.view_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
				FROM information_schema.columns 
				WHERE table_schema=rec.scheme_name and table_name=rec.name_db and column_name=rec.pk_fileld;
			IF seq_name<>'' AND seq_name is not null THEN
				raise notice 'SQL:: %', 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
				EXECUTE 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			END IF;	
		END IF;
		FOR rec2 IN SELECT scheme_name, history_table_name FROM sys_scheme.table_history_info thi, sys_scheme.table_info ti 
				WHERE ti.id = thi.id_table AND ti.id = rec.id LOOP
			SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like rec2.history_table_name
						AND table_type like 'BASE TABLE') INTO exists_table_val;
					IF exists_table_val=true THEN
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec2.history_table_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			/*SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
				FROM information_schema.columns 
				WHERE table_schema=rec.scheme_name and table_name=rec2.history_table_name and column_name='id_history';
			IF seq_name<>'' AND seq_name is not null THEN
				raise notice 'SQL:: %', 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
				EXECUTE 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			END IF;	*/
		END IF;
		END LOOP;
	END LOOP;

	FOR rec IN SELECT id, scheme_name, name_db FROM sys_scheme.table_info LOOP
		SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like 'photo_'||rec.name_db
						AND table_type like 'BASE TABLE') INTO exists_table_val;
		IF exists_table_val=true THEN
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.photo_'||rec.name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
				FROM information_schema.columns 
				WHERE table_schema=rec.scheme_name and table_name='photo_'||rec.name_db and column_name='id';
			IF seq_name<>'' AND seq_name is not null THEN
				EXECUTE 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			END IF;	
		END IF;
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  

CREATE TABLE sys_scheme.fts_tables
(
  id_table integer NOT NULL,
  display_text text,
  enabled boolean NOT NULL DEFAULT true,
  CONSTRAINT fts_tables_pkey PRIMARY KEY (id_table),
  CONSTRAINT fts_tables_id_table_fkey FOREIGN KEY (id_table)
      REFERENCES sys_scheme.table_info (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

CREATE TABLE sys_scheme.fts_index
(
  id_table integer NOT NULL,
  pkid bigint NOT NULL,
  fts tsvector,
  label_text text,
  CONSTRAINT fts_index_pkey PRIMARY KEY (id_table, pkid),
  CONSTRAINT fts_index_id_table_fkey FOREIGN KEY (id_table)
      REFERENCES sys_scheme.fts_tables (id_table) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
CREATE INDEX fts_index_fts
  ON sys_scheme.fts_index
  USING gin
  (fts);
  

CREATE TABLE sys_scheme.fts_fields
(
  id serial NOT NULL,
  id_table integer,
  id_field integer,
  order_num integer,
  CONSTRAINT fts_fields_pkey PRIMARY KEY (id),
  CONSTRAINT fts_fields_id_field_fkey FOREIGN KEY (id_field)
      REFERENCES sys_scheme.table_field_info (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fts_fields_id_table_fkey FOREIGN KEY (id_table)
      REFERENCES sys_scheme.fts_tables (id_table) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fts_fields_id_table_id_field_key UNIQUE (id_table, id_field)
)
WITH (
  OIDS=FALSE
);
CREATE INDEX fki_fts_fields_index
  ON sys_scheme.fts_fields
  USING btree
  (id_table);
CREATE OR REPLACE FUNCTION sys_scheme.fts_prepareindex(in_table_id integer)
  RETURNS void AS
$BODY$DECLARE
intquery character varying;
addquery character varying;
table_name character varying;
layer_record RECORD;
fields_record record;
firstrow boolean;
cchar char;
cstart int;
BEGIN
	FOR layer_record IN SELECT st.id_table, st.display_text, st.enabled, ti.pk_fileld, ti.scheme_name, ti.name_db
									FROM sys_scheme.fts_tables st, sys_scheme.table_info ti 
									WHERE st.id_table=in_table_id AND st.enabled = TRUE AND ti.id =st.id_table
									LOOP
		DELETE FROM sys_scheme.fts_index WHERE id_table=in_table_id;
		intquery = 'SELECT '||layer_record.id_table||' as id_table,
		"' || layer_record.pk_fileld || '" as gid, 
		' || layer_record.display_text  || ' as label, ';
		firstrow = true;
		cchar = 'A';
		cstart:=ascii(cchar);		
		addquery = '';
		FOR fields_record IN SELECT name_db 
								FROM sys_scheme.fts_fields tf, sys_scheme.table_field_info tfi
								WHERE tf.id_table=layer_record.id_table AND tfi.id = tf.id_field 
								ORDER BY tf.order_num 
								LOOP
			IF firstrow=false THEN
				addquery = ' || ';
			ELSEIF firstrow = true THEN
				firstrow=false;
			END IF;
			
			intquery = intquery || addquery || 'setweight( COALESCE( to_tsvector( ''russian'', "' || fields_record.name_db || '"::character varying), ''''), '''|| cchar || ''')';
			cstart=cstart+1;
			cchar=chr(cstart);
		END LOOP;
		intquery = intquery || ' FROM  "' || layer_record.scheme_name || '"."' || layer_record.name_db || '"';
		RAISE NOTICE 'query: %',intquery;

		EXECUTE 'INSERT INTO sys_scheme.fts_index (id_table, pkid, label_text, fts) '||intquery;
		
		/*INSERT INTO sys_scheme.fts_index (layer_id,pkid,fts,point,label) 
					SELECT layer_record.id_table,gid,fts,geom,label FROM dblink('fts_conn', intquery) AS t1(gid int, geom geometry, label character varying,fts tsvector);
		*/			
	END LOOP;
	table_name = sys_scheme.get_table_name(in_table_id);
	intquery = 'CREATE TRIGGER change_' || table_name || E'_index  AFTER INSERT OR UPDATE OR DELETE ON \"' || 
		sys_scheme.get_table_scheme_name(in_table_id) || E'\".\"' || table_name ||
		E'\" FOR EACH ROW EXECUTE PROCEDURE sys_scheme.trg_after_change_index();';
	RAISE NOTICE 'query: %', intquery;
	EXECUTE intquery;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

-------------------------------------------------------------------------

-- Function: sys_scheme.fts_deleteindex(integer)

-- DROP FUNCTION sys_scheme.fts_deleteindex(integer);

CREATE OR REPLACE FUNCTION sys_scheme.fts_deleteindex(in_table_id integer)
  RETURNS void AS
$BODY$DECLARE
intquery character varying;
table_name character varying;
table_scheme_name character varying;
BEGIN
	table_name = sys_scheme.get_table_name(in_table_id);
	table_scheme_name = sys_scheme.get_table_scheme_name(in_table_id);
	intquery = 'DELETE FROM sys_scheme.fts_tables WHERE id_table = ' || in_table_id || ';' ||
		'DROP TRIGGER change_' || table_name || '_index ' || E'ON \"' || table_scheme_name || E'\".\"' || table_name || E'\";';
	RAISE NOTICE 'query: %', intquery;
	EXECUTE intquery;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

----------------------------------------------------------------------------

-- Function: sys_scheme.trg_after_change_index()

-- DROP FUNCTION sys_scheme.trg_after_change_index();

CREATE OR REPLACE FUNCTION sys_scheme.trg_after_change_index()
  RETURNS trigger AS
$BODY$
DECLARE
	fts character varying;
	fts_fields character varying;
	query character varying;
	display_text character varying;
	name_field character varying;
	addquery character varying;
	table_id int;
	pk_id int;
	layer_record RECORD;
	fields_record record;
	firstrow boolean;
	cchar char;
	cstart int;
BEGIN

	SELECT id INTO table_id FROM sys_scheme.table_info WHERE scheme_name = TG_TABLE_SCHEMA AND name_db = TG_TABLE_NAME;
	
	SELECT st.id_table, st.display_text, st.enabled, ti.pk_fileld, ti.scheme_name, ti.name_db
	INTO layer_record 
	FROM sys_scheme.fts_tables st, sys_scheme.table_info ti 
	WHERE st.id_table=table_id AND st.enabled = TRUE AND ti.id = st.id_table;

	IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
    
		EXECUTE 'CREATE TEMPORARY TABLE new_row (LIKE ' ||
			quote_ident(TG_TABLE_SCHEMA) || '.' || quote_ident(TG_TABLE_NAME) ||
			') ON COMMIT DROP';

		INSERT INTO new_row SELECT NEW.*;

		firstrow = true;
		cchar = 'A';
		cstart:=ascii(cchar);		
		addquery = '';
		fts_fields = '';
		FOR fields_record IN 
			SELECT name_db 
			FROM sys_scheme.fts_fields tf, sys_scheme.table_field_info tfi
			WHERE tf.id_table = layer_record.id_table AND tfi.id = tf.id_field 
			ORDER BY tf.order_num 
			LOOP
				IF firstrow=false THEN
					addquery = ' || ';
				ELSEIF firstrow = true THEN
					firstrow=false;
				END IF;
					
				fts_fields = fts_fields || addquery || 'setweight( to_tsvector( ''russian'', "' || fields_record.name_db || '"::character varying), '''|| cchar || ''')';
				cstart=cstart+1;
				cchar=chr(cstart);
			END LOOP;
			
		IF fts_fields LIKE '' OR fts_fields IS NULL THEN
			RETURN NEW;
		END IF;
		
		fts = ' (SELECT ' || fts_fields || ' FROM new_row)';
			
		EXECUTE 'SELECT ' || layer_record.display_text || ' FROM new_row' INTO display_text;
		EXECUTE 'SELECT ' || layer_record.pk_fileld || ' FROM new_row' INTO pk_id;

	END IF;

	IF TG_OP = 'INSERT' THEN
		query = 'INSERT INTO sys_scheme.fts_index (id_table, pkid, fts, label_text) VALUES (' || 
			layer_record.id_table || ',' || pk_id || ',' || fts || E',\'' || COALESCE(display_text, '') || E'\');';
		RAISE NOTICE 'query: %', query;
		
		EXECUTE query;	
		DROP TABLE new_row;

		RETURN NEW;
		
	ELSIF TG_OP = 'UPDATE' THEN
		query = 'UPDATE sys_scheme.fts_index SET fts = '|| fts || E', label_text = \'' || COALESCE(display_text, '') || E'\' WHERE id_table = ' || layer_record.id_table || ' AND pkid = ' || pk_id || ';';
		RAISE NOTICE 'query: %', query;
		
		EXECUTE query;	
		DROP TABLE new_row;

		RETURN NEW;
				
	ELSIF TG_OP = 'DELETE' THEN
		EXECUTE 'SELECT $1.' || layer_record.pk_fileld INTO pk_id USING OLD;
		
		query = 'DELETE FROM sys_scheme.fts_index  
			WHERE id_table = ' || layer_record.id_table || ' AND pkid = ' || pk_id || ';';
		RAISE NOTICE 'query: %', query;
		
		EXECUTE query;	

		RETURN OLD;	

	END IF;

END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

  
CREATE OR REPLACE FUNCTION sys_scheme.set_action_right(action_id integer, user_id integer, can boolean)
  RETURNS boolean AS
$BODY$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.actions_users WHERE id_action = action_id AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.actions_users SET allowed = can WHERE id_action = action_id AND id_user = user_id;
	ELSE
		INSERT INTO sys_scheme.actions_users (id_action, id_user, allowed) VALUES (action_id, user_id, can);
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
COMMENT ON FUNCTION sys_scheme.set_action_right(integer, integer, boolean) IS 'Назначение прав на действие пользователю Инфраструктуры';

ALTER TABLE sys_scheme.actions_users ADD PRIMARY KEY (id);
ALTER TABLE sys_scheme.actions_users DROP COLUMN read;
ALTER TABLE sys_scheme.actions_users DROP COLUMN write;
ALTER TABLE sys_scheme.actions_users ADD COLUMN allowed boolean;
ALTER TABLE sys_scheme.actions_users ALTER COLUMN allowed SET DEFAULT false;

CREATE INDEX sys_scheme_table_right_id_table_index ON sys_scheme.table_right(id_table);
CREATE INDEX sys_scheme_table_right_id_user_index ON sys_scheme.table_right(id_user);
CREATE INDEX sys_scheme_actions_users_id_user_index ON sys_scheme.actions_users(id_user);
CREATE INDEX sys_scheme_actions_users_id_action_index ON sys_scheme.actions_users(id_action);

ALTER TABLE sys_scheme.actions_users DROP CONSTRAINT actions_users_id_action_fkey;

ALTER TABLE sys_scheme.actions_users
  ADD CONSTRAINT actions_users_id_action_fkey FOREIGN KEY (id_action)
      REFERENCES sys_scheme.action (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;

INSERT INTO sys_scheme.action(id, name, table_data, name_visible, operation) VALUES (1, 'Export', FALSE, 'Экспорт', FALSE);
INSERT INTO sys_scheme.action(id, name, table_data, name_visible, operation) VALUES (2, 'Import', FALSE, 'Импорт', FALSE);


CREATE OR REPLACE FUNCTION sys_scheme.get_ref_tables(table_id INTEGER)
  RETURNS SETOF INTEGER AS
$BODY$
DECLARE
r INTEGER;
BEGIN
	FOR r IN 
		(SELECT ref_table
		FROM sys_scheme.table_field_info 
		WHERE id_table=table_id AND ref_table IS NOT NULL)
	LOOP
	  RETURN NEXT r;
	 END LOOP;
	 RETURN;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;
ALTER FUNCTION sys_scheme.get_group_list() OWNER TO postgres;
COMMENT ON FUNCTION sys_scheme.get_group_list() IS 'Возвращает id таблиц, на которых ссылается заданная таблица';

CREATE OR REPLACE FUNCTION sys_scheme.set_action_right(action_id integer, user_id integer, can boolean)
  RETURNS boolean AS
$BODY$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.actions_users WHERE id_action = action_id AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.actions_users SET allowed = can WHERE id_action = action_id AND id_user = user_id;
	ELSE
		INSERT INTO sys_scheme.actions_users (id_action, id_user, allowed) VALUES (action_id, user_id, can);
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sys_scheme.set_action_right(integer, integer, boolean)  OWNER TO postgres;
COMMENT ON FUNCTION sys_scheme.set_action_right(integer, integer, boolean) IS 'Назначение прав на действие пользователю Инфраструктуры';

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
  
  
  
CREATE TABLE sys_scheme.report_templates
(
  id serial NOT NULL,
  type_report integer,
  body text,
  caption text NOT NULL,
  id_table integer,
  CONSTRAINT report_templates_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);


CREATE OR REPLACE FUNCTION sys_scheme.get_sql_for_table_with_geom(id_table_val integer, srid_map integer)
  RETURNS character varying AS
$BODY$DECLARE
__scheme text;
__table_name text;
__geom_type integer;

__rec_field RECORD;
__index integer = 0;
__ref_type integer;

__column text = '';
__alias text = '';
__alias_id text = '';

__ref_table text = '';
__ref_field text = '';
__ref_field_end text = '';
__ref_field_name text = '';
__ref_id text = '';

__select text;
__select_id text;
__select_geom text;
__from text;

__list_replace text[][2];

__select_full text = '';
__select_id_full text = '';
__select_geom_full text = '';
__from_full text = '';

__sql_full text = '';
BEGIN

-- quote_ident("column") - инъекция названий в базе таблиц и колонок
		SELECT quote_ident("scheme_name"), quote_ident("name_db"), "geom_type"
		INTO __scheme, __table_name, __geom_type 
		FROM sys_scheme.table_info 
		WHERE "id" = "id_table_val";

	__from_full := CONCAT(E'\t', __scheme, '.', __table_name, ' ___t$');
	
	FOR __rec_field 
	IN 
		SELECT "name_db", "type_field", "is_reference", "is_interval", "ref_table", "ref_field", "ref_field_end", "ref_field_name"
		FROM sys_scheme.table_field_info
		WHERE "id_table" = "id_table_val" AND "visible" = TRUE ORDER BY "num_order" 
	LOOP
		IF (__rec_field.is_reference = TRUE  AND __rec_field.is_interval = FALSE)
		THEN
			__ref_type := 1;
		ELSIF (__rec_field.is_reference = FALSE  AND __rec_field.is_interval = TRUE)
		THEN
			__ref_type := 2;
		ELSE
			__ref_type := 0;
		END IF;


		-- Начало формирование строк SELECT и FROM для конкретнго поля
		__index := __index + 1;
		__column := '___t$.' || quote_ident(__rec_field.name_db);
		__alias := quote_ident(__rec_field.name_db);
		__alias_id := quote_ident('id!' || __rec_field.name_db);

		__ref_table := quote_ident(sys_scheme.get_table_scheme_name(__rec_field.ref_table)) || '.' || quote_ident(sys_scheme.get_table_name(__rec_field.ref_table));
		__ref_field := quote_ident(sys_scheme.get_table_field_name(__rec_field.ref_field));
		__ref_field_end := quote_ident(sys_scheme.get_table_field_name(__rec_field.ref_field_end));
		__ref_field_name := quote_ident(sys_scheme.get_table_field_name(__rec_field.ref_field_name));
		__ref_id := quote_ident(sys_scheme.get_table_pkfield(__rec_field.ref_table));

		__select := CONCAT(E'\t', __column, ' AS ', __alias);
		__select_id := '';
		__select_geom := '';
		__from := '';

		--RAISE NOTICE 'Column:: %', __column;
		IF 	(__ref_type = 1 OR __ref_type = 4) -- справочник
		THEN 

			__select := CONCAT(E'\t___r$', __index, '.', __ref_field_name, ' AS ', __alias);

			__select_id := CONCAT(E'\t', __column, ' AS ', __alias_id);

			__from := CONCAT(E'\tLEFT JOIN ', __ref_table, ' ___r$', __index, E'\n') 
						|| CONCAT(E'\t\t ON (', __column, ' = ___r$', __index, '.', __ref_field, ')');


 		ELSIF 	(__ref_type = 2) -- интервал
		THEN 			
 			__select := CONCAT(E'\tCOALESCE(___i$', __index, '.', __ref_field_name, ' || ''('' || ', __column, ' || '')'', ', __column, '::text) AS ', __alias);
 			
			__select_id := CONCAT(E'\t', __column, ' AS ', __alias_id);
 
 			__from := CONCAT(E'\t LEFT JOIN ', __ref_table, ' ___i$', __index, E'\n')
						|| CONCAT(E'\t\t ON ___i$', __index, '.', __ref_id, E' = (\n') 
						|| CONCAT(E'\t\t\t SELECT ___v$', __index, '.', __ref_id, E'\n')
						|| CONCAT(E'\t\t\t FROM ', __ref_table, ' ___v$', __index, E'\n')
						|| CONCAT(E'\t\t\t WHERE \n')
						|| CONCAT(E'\t\t\t\t ___v$', __index, '.', __ref_field, ' < ', __column, E'\n')
						|| CONCAT(E'\t\t\t\t AND ', __column, ' <= ___v$', __index, '.', __ref_field_end, E'\n')
						|| CONCAT(E'\t\t\t LIMIT 1)');

			
		ELSIF 	(__ref_type <> 0)
		THEN
			__select := E'\t--- ошибка типа ref_type';
			__select_id := E'\t---ошибка типа ref_type';
			__from := E'\t---ошибка типа ref_type';
		END IF;

		IF 	(__rec_field.type_field = 5)
 		THEN
 			__select := '';

			__select_geom := CONCAT(E'\tgeometrytype(st_transform(', __column, ', ', srid_map , E')) AS "geom!type",\n');
			IF 	(__geom_type = 3)
			THEN	
				__select_geom := CONCAT(__select_geom, E'\tCASE\tWHEN ST_ISVALID(', __column, E')=TRUE\n\t\tTHEN ST_AREA(geography(st_transform(', __column, E', 4326)))\n\t\tELSE NULL\n\t\tEND AS "geom!area",\n');
				__select_geom := CONCAT(__select_geom, E'\tST_PERIMETER(st_transform(', __column, ', ', srid_map , E')) AS "geom!perimeter",\n');
			ELSIF	(__geom_type = 2)
			THEN
				__select_geom := CONCAT(__select_geom, E'\tST_LENGTH(geography(st_transform(', __column, E', 4326))) AS "geom!length",\n');
			END IF;

			__select_geom := CONCAT(__select_geom, E'\tCASE\tWHEN ST_ISVALID(', __column, E')=TRUE\n\t\tTHEN ST_X(ST_CENTROID(st_transform(', __column, ', ', srid_map , E')))\n\t\tELSE NULL\n\t\tEND AS "geom!center_x",\n');
			__select_geom := CONCAT(__select_geom, E'\tCASE\tWHEN ST_ISVALID(', __column, E')=TRUE\n\t\tTHEN ST_Y(ST_CENTROID(st_transform(', __column, ', ', srid_map , E')))\n\t\tELSE NULL\n\t\tEND AS "geom!center_y"');
 		END IF;

		-- Объединение строк в общий запрос

		--RAISE NOTICE 'SELECT:: %,    %', __select, CONCAT(__select_id, E'\t', 'ST_X(ST_CENTROID(st_transform(', __column, ', ', srid_map, '))) AS ', '"geom!center_x"', ',', E'\n');

		-- Строка SELECT 
		IF 	(__select_full <> '' AND __select <> '') 
		THEN 	__select_full := __select_full || E',\n' || __select;
		ELSE 	__select_full := __select_full || __select;
		END IF;
		

		-- Строка SELECT с айдишниками
		IF 	(__select_id_full <> '' AND __select_id <> '') 
		THEN 	__select_id_full := __select_id_full || E',\n' || __select_id;
		ELSE 	__select_id_full := __select_id_full || __select_id;
		END IF;
		
		RAISE NOTICE '__select_geom:: %', __select_geom; 
		-- Строка SELECT с дополнительными данными
		IF 	(__select_geom_full <> '' AND __select_geom <> '') 
		THEN 	__select_geom_full := __select_geom_full || E',\n' || __select_geom;
		ELSE 	__select_geom_full := __select_geom_full || __select_geom;
		END IF;
		
		-- Строка FROM с дополнительными данными
		IF 	(__from_full <> '' AND __from <> '') 
		THEN 	__from_full := __from_full || E'\n' || __from;
		ELSE 	__from_full := __from_full || __from;
		END IF;
		
	END LOOP;

	IF 	(__index = 0) 
	THEN 	RETURN '';
	END IF;
	
	-- Формирование конечного SQL запроса
	IF (__select_id_full != '') 
	THEN
		__select_full = CONCAT(__select_full, E', \n', __select_id_full);
	END IF;

	IF (__select_geom_full != '') 
	THEN
		__select_full = CONCAT(__select_full, E', \n', __select_geom_full);
	END IF;

	__sql_full := E'SELECT \n' || __select_full || E' \nFROM \n' || __from_full;
	RAISE NOTICE '%', __sql_full;
	RETURN __sql_full;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;  

CREATE OR REPLACE FUNCTION sys_scheme.trg_after_fts_field_delete()
  RETURNS trigger AS
$BODY$
DECLARE
	count int;
BEGIN
	SELECT count(*) 
	INTO count
	FROM sys_scheme.fts_fields
	WHERE id_table = OLD.id_table;

	IF count = 0 THEN
		DELETE FROM sys_scheme.fts_fields
		WHERE id_table = OLD.id_table;
	END IF;
END; 
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE TRIGGER del_after_field
  BEFORE DELETE
  ON sys_scheme.fts_fields
  FOR EACH ROW
  EXECUTE PROCEDURE sys_scheme.trg_after_fts_field_delete();  

CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_8_4_0_to_2_9_0_0()
  RETURNS boolean AS
$BODY$DECLARE
exists_version_new boolean;
exists_version_old boolean;
major_val_new integer;
minor_val_new integer;
build_val_new integer;
revision_val_new integer;
major_val_old integer;
minor_val_old integer;
build_val_old integer;
revision_val_old integer;
version_seq_new integer;
BEGIN
	major_val_new:=2;
	minor_val_new:=9;
	build_val_new:=0;
	revision_val_new:=0;
	
	version_seq_new:=3;
	
	major_val_old:=2;
	minor_val_old:=8;
	build_val_old:=4;
	revision_val_old:=0;
	
		select exists(SELECT true FROM sys_scheme.db_version WHERE major = major_val_new 
						AND minor = minor_val_new 
						AND build=build_val_new 
						AND revision =revision_val_new) INTO exists_version_new;
		IF exists_version_new = true THEN
			RAISE EXCEPTION 'Сообщение: %', 'Это обновление уже было!';
			return false;
		END IF;
		
		select exists(SELECT true FROM sys_scheme.db_version WHERE major = major_val_old 
						AND minor = minor_val_old 
						AND build=build_val_old 
						AND revision =revision_val_old) INTO exists_version_old;
		IF exists_version_old = false THEN
			RAISE EXCEPTION 'Сообщение: %', 'Необходимо сначало обновить до версии 2.8.4!';
			return false;
		END IF;
		
		raise notice 'Сообщение: %', 'Фиксируем факт обновления';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice 'Сообщение: %', 'Обновление прошло успешно!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_8_4_0_to_2_9_0_0();
  -- запуск фикс ДБ  
SELECT sys_scheme.super_fix_db();
