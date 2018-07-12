--
-- PostgreSQL database dump
--

-- Dumped from database version 9.1.4
-- Dumped by pg_dump version 9.2.2
-- Started on 2013-04-01 16:10:36

SET statement_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = off;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET escape_string_warning = off;

--
-- TOC entry 6 (class 2615 OID 36965)
-- Name: sys_scheme; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA sys_scheme;


SET search_path = sys_scheme, pg_catalog;

--
-- TOC entry 1472 (class 1247 OID 36968)
-- Name: atribut_info; Type: TYPE; Schema: sys_scheme; Owner: -
--

CREATE TYPE atribut_info AS (
	field_name character varying,
	field_sys_name character varying,
	field_val character varying,
	data_type integer
);


--
-- TOC entry 1475 (class 1247 OID 36971)
-- Name: group_info; Type: TYPE; Schema: sys_scheme; Owner: -
--

CREATE TYPE group_info AS (
	id integer,
	name_group character varying,
	descript character varying,
	order_num integer
);


--
-- TOC entry 1644 (class 1247 OID 68578)
-- Name: history_list_by_user; Type: TYPE; Schema: sys_scheme; Owner: -
--

CREATE TYPE history_list_by_user AS (
	user_name character varying,
	data_changes timestamp without time zone,
	id_history integer,
	type_operation integer,
	id_history_table integer,
	id_table integer,
	id_object integer
);


--
-- TOC entry 1478 (class 1247 OID 36977)
-- Name: style_info; Type: TYPE; Schema: sys_scheme; Owner: -
--

CREATE TYPE style_info AS (
	scheme character varying,
	tabel_name character varying,
	field_name character varying,
	type_geom integer,
	type_style integer,
	type_val smallint,
	reference_int_val integer,
	interval_begin_int_val integer,
	interval_end_int_val integer,
	interval_begin_numeric_val numeric,
	interval_end_numeric_val numeric,
	fontname character varying,
	fontcolor integer,
	fontframecolor integer,
	fontsize integer,
	symbol integer,
	pencolor integer,
	pentype integer,
	penwidth integer,
	brushbgcolor bigint,
	brushfgcolor integer,
	brushstyle integer,
	brushhatch integer,
	range_colors boolean,
	range_column character varying,
	precision_point integer,
	type_color integer,
	min_color bigint,
	min_val integer,
	max_color bigint,
	max_val integer,
	use_min_val boolean,
	null_color bigint,
	use_null_color boolean,
	use_max_val boolean
);


--
-- TOC entry 1117 (class 1255 OID 36978)
-- Name: add_group(character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION add_group(name_group_val character varying, descript_val character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
exists_item boolean;
id_new_group integer;
BEGIN
       id_new_group = -1;
       SELECT EXISTS(SELECT 1 FROM sys_scheme.table_groups WHERE name_group
like name_group_val) INTO exists_item;

       IF exists_item = FALSE THEN
               SELECT nextval('sys_scheme.table_groups_id_seq') INTO id_new_group;
               INSERT INTO sys_scheme.table_groups(id, name_group, descript) VALUES
(id_new_group, name_group_val, descript_val);
       ELSE
               SELECT id INTO id_new_group FROM sys_scheme.table_groups WHERE
name_group like name_group_val;
               RETURN id_new_group;
       END IF;
RETURN id_new_group;
END;$$;


--
-- TOC entry 3443 (class 0 OID 0)
-- Dependencies: 1117
-- Name: FUNCTION add_group(name_group_val character varying, descript_val character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION add_group(name_group_val character varying, descript_val character varying) IS 'Добавление новой группы';


--
-- TOC entry 1027 (class 1255 OID 36979)
-- Name: add_right_to_table(character varying, character varying, integer, character varying, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION add_right_to_table(scheme character varying, table_name character varying, user_id integer, right_type character varying, val boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
pk_field character varying;
user_name character varying;
--exists_photo boolean;
BEGIN
	SELECT ti.pk_fileld INTO pk_field FROM sys_scheme.table_info ti WHERE ti.scheme_name = scheme AND ti.name_db = table_name;
	--SELECT ti.photo INTO exists_photo FROM sys_scheme.table_info ti WHERE ti.scheme_name = scheme AND ti.name_db = table_name;
	SELECT ud."login" INTO user_name FROM sys_scheme.user_db ud WHERE ud.id = user_id;
	IF val=true THEN
		EXECUTE 'GRANT '||right_type||' ON '||scheme||'."'||table_name||'" TO ' || user_name;
	END IF;
	IF val=false THEN
		EXECUTE 'REVOKE '||right_type||' ON '||scheme||'."'||table_name||'" FROM ' || user_name;
	END IF;
RETURN true;
END;$$;


--
-- TOC entry 3444 (class 0 OID 0)
-- Dependencies: 1027
-- Name: FUNCTION add_right_to_table(scheme character varying, table_name character varying, user_id integer, right_type character varying, val boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION add_right_to_table(scheme character varying, table_name character varying, user_id integer, right_type character varying, val boolean) IS 'Работа с правами на таблицу GRANT & REVOKE';


--
-- TOC entry 1028 (class 1255 OID 36980)
-- Name: add_table_in_group(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION add_table_in_group(table_id integer, group_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_groups_table WHERE id_table = table_id AND id_group = group_id) INTO exists_item;
	IF exists_item = FALSE THEN
		INSERT INTO sys_scheme.table_groups_table(id_table, id_group) VALUES (table_id, group_id);
	END IF;
RETURN true;
END;$$;


--
-- TOC entry 3445 (class 0 OID 0)
-- Dependencies: 1028
-- Name: FUNCTION add_table_in_group(table_id integer, group_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION add_table_in_group(table_id integer, group_id integer) IS 'Добавление таблицы в группу';


--
-- TOC entry 1122 (class 1255 OID 37619)
-- Name: change_table_group(integer, character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION change_table_group(table_id integer, source_groupname character varying, dest_groupname character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
id_group_val integer;
BEGIN
       IF source_groupname IS NOT NULL THEN
               id_group_val:=sys_scheme.add_group(source_groupname, source_groupname);
               DELETE FROM sys_scheme.table_groups_table WHERE id_table = table_id
AND id_group=id_group_val;
       END IF;
       id_group_val:=sys_scheme.add_group(dest_groupname, dest_groupname);
       PERFORM sys_scheme.add_table_in_group(table_id, id_group_val);
RETURN id_group_val;
END;$$;


--
-- TOC entry 3446 (class 0 OID 0)
-- Dependencies: 1122
-- Name: FUNCTION change_table_group(table_id integer, source_groupname character varying, dest_groupname character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION change_table_group(table_id integer, source_groupname character varying, dest_groupname character varying) IS 'Изменение группы у таблицы';


--
-- TOC entry 1132 (class 1255 OID 36981)
-- Name: create_admin_group(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_admin_group() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
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
	EXECUTE 'GRANT ALL ON sys_scheme.user_params TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_info_sets TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.sets_styles TO ' || admin_group_name;
	RETURN true;
END;$$;


--
-- TOC entry 3447 (class 0 OID 0)
-- Dependencies: 1132
-- Name: FUNCTION create_admin_group(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_admin_group() IS 'Создание группы администраторов';


--
-- TOC entry 1129 (class 1255 OID 36982)
-- Name: create_client_group(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_client_group() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
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
	EXECUTE 'GRANT ALL ON sys_scheme.table_history_photo_id_seq TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.tokens TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_files TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.upd_journal TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_updater TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.upd_user_files TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.user_params TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.table_info_sets TO ' || client_group_name;
	EXECUTE 'GRANT SELECT ON sys_scheme.sets_styles TO ' || client_group_name;
	RETURN true;
END;$$;


--
-- TOC entry 1029 (class 1255 OID 36983)
-- Name: create_exists_field(integer, character varying, character varying, integer, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_exists_field(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
table_sheme_name character varying;
table_name_db character varying;
type_field_name character varying;
sql_string character varying;
type_geom_name character varying;
id_new_field integer;
BEGIN
	SELECT name_db INTO table_name_db FROM sys_scheme.table_info WHERE id = table_id;
	SELECT scheme_name INTO table_sheme_name FROM sys_scheme.table_info WHERE id = table_id;
	SELECT name_db INTO type_field_name FROM sys_scheme.table_type WHERE id = field_type;
	SELECT ttg.namedb INTO type_geom_name FROM sys_scheme.table_info ti, sys_scheme.table_type_geom ttg WHERE ti.id=table_id AND ttg.id = ti.geom_type;

	
	SELECT nextval('sys_scheme.table_field_info_id_seq') INTO id_new_field;
	INSERT INTO sys_scheme.table_field_info(id, id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
	VALUES (id_new_field, table_id, field_name_db, field_name_map, field_type, true, field_name_lable, false, false);
RETURN id_new_field;
END;$$;


--
-- TOC entry 1030 (class 1255 OID 36984)
-- Name: create_exists_table(character varying, character varying, character varying, character varying, integer, integer, boolean, boolean, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_exists_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, pk_field_name character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
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
                                      'CONSTRAINT photo_'||table_name_db||'_pkey PRIMARY KEY (id), '||
                                      ' CONSTRAINT photo_'||table_name_db ||'_id_obj_fkey FOREIGN KEY (id_obj) '||
                                      ' REFERENCES '|| table_scheme_name ||'.'|| table_name_db ||' ('||pk_field_name||') MATCH SIMPLE '||
                                      ' ON UPDATE CASCADE ON DELETE CASCADE '||
                                    ') '||
                                    'WITH ('||
                                      'OIDS=FALSE'||
                                    '); '||
				    'ALTER TABLE '|| table_scheme_name ||'.photo_'|| table_name_db ||' OWNER TO ' || sys_scheme.get_admin_group() ||'; '||
                                    'GRANT ALL ON TABLE '|| table_scheme_name ||'.photo_'|| table_name_db ||' TO public; '||
                                    'CREATE TRIGGER insert_history_photo_'||table_name_db||' '||
				    'BEFORE INSERT OR UPDATE OR DELETE '||
				    'ON '||table_scheme_name||'.photo_'||table_name_db||' '||
				    'FOR EACH ROW '||
				    'EXECUTE PROCEDURE sys_scheme.history_photo_insrt();';
	END IF;
END IF;

INSERT INTO sys_scheme.table_info (id, name_db, name_map, "geom_type", "type", map_style, "scheme_name", "photo", pk_fileld)
	VALUES (id_new_table, table_name_db, table_name_sys, table_geom_type, table_type, is_style, table_scheme_name, photo_exists, pk_field_name);

IF photo_exists = TRUE THEN
INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)
                                        VALUES (id_new_table, 'photo_'||table_name_db, 'id_obj', 'file', 'id');
END IF;

/*INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval, num_order) 
			VALUES (id_new_table, pk_field_name, pk_field_name, 1, true, 'Номер', false, false, 1);
IF table_type = 1 THEN
	INSERT INTO sys_scheme.table_field_info(id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval, num_order)
                   VALUES (id_new_table, 'geom', 'geom', 5, true, 'geom', false, false, 2);
END IF;*/

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
                                                            'ADD brushbgcolor integer DEFAULT 16711680,'||
                                                            'ADD brushfgcolor integer DEFAULT 16711680,'||
                                                            'ADD brushstyle integer DEFAULT 0,'||
                                                            'ADD brushhatch integer DEFAULT 1;'||
                                                            'ALTER TABLE '||table_scheme_name||'.'|| table_name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
END IF;
PERFORM sys_scheme.set_table_rights_for_admins(id_new_table);
IF table_group IS NOT NULL AND table_group>0 THEN
	INSERT INTO sys_scheme.table_groups_table (id_table, id_group) VALUES (id_new_table, table_group);
END IF;
RETURN id_new_table;

END;$$;


--
-- TOC entry 3448 (class 0 OID 0)
-- Dependencies: 1030
-- Name: FUNCTION create_exists_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, pk_field_name character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_exists_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, pk_field_name character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer) IS 'Прикрепление существующей таблицы';


--
-- TOC entry 1031 (class 1255 OID 36985)
-- Name: create_exists_table_get_info(character varying, character varying, character varying, character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_exists_table_get_info(scheme_name_db character varying, table_name_db character varying, table_name_map character varying, pk_field character varying, geom_field_f character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
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
--raise notice '����� %', '1';
IF id_table_new>0 THEN
--raise notice '����� %', '2';
	UPDATE sys_scheme.table_info SET geom_field = geom_field_f WHERE id = id_table_new;
	FOR r IN SELECT * FROM information_schema.columns 
		WHERE table_schema like scheme_name_db AND table_name like table_name_db AND
		(data_type like 'integer%' OR data_type like 'character varying' OR data_type like 'numeric' OR data_type like 'USER-DEFINED'  OR data_type like 'date') 
			/*AND (column_name not like pk_field_temp AND column_name not like geom_field_f)*/ LOOP
		IF r.data_type = 'integer' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 1, r.column_name);
		--END IF;
		ELSIF r.data_type = 'character varying' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 2, r.column_name);
		--END IF;
		ELSIF r.data_type = 'numeric' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 6, r.column_name);

		ELSIF r.data_type = 'USER-DEFINED' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 5, r.column_name);
		ELSIF r.data_type = 'date' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 3, r.column_name);
		END IF;
		--raise notice '����� %', r.column_name;
	END LOOP;
END IF;

RETURN id_table_new;
END;$$;


--
-- TOC entry 1032 (class 1255 OID 36986)
-- Name: create_field(integer, character varying, character varying, integer, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_field(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
table_sheme_name character varying;
table_name_db character varying;
type_field_name character varying;
sql_string character varying;
sql_string_history character varying;
type_geom_name character varying;
id_new_field integer;
exists_history boolean;
BEGIN
	SELECT name_db INTO table_name_db FROM sys_scheme.table_info WHERE id = table_id;
	SELECT scheme_name INTO table_sheme_name FROM sys_scheme.table_info WHERE id = table_id;
	SELECT name_db INTO type_field_name FROM sys_scheme.table_type WHERE id = field_type;
	SELECT ttg.namedb INTO type_geom_name FROM sys_scheme.table_info ti, sys_scheme.table_type_geom ttg WHERE ti.id=table_id AND ttg.id = ti.geom_type;
	sql_string:='ALTER TABLE '||table_sheme_name||'.'||table_name_db||' ADD COLUMN "'||field_name_db||'" ';
	sql_string_history:='ALTER TABLE '||table_sheme_name||'.'||table_name_db||'_history ADD COLUMN "'||field_name_db||'" ';
	IF field_type = 1 OR field_type = 6 THEN
		sql_string:=sql_string||type_field_name||';';
		sql_string_history:= sql_string_history||type_field_name||';';
	END IF;
	IF field_type = 2 THEN
		sql_string:=sql_string||type_field_name||';';
		sql_string_history:=sql_string_history||type_field_name||';';
	END IF;
	IF field_type = 3 OR field_type = 4 THEN
		sql_string:=sql_string||type_field_name||';';
		sql_string_history:=sql_string_history||type_field_name||';';
	END IF;
	IF field_type = 5 THEN
		sql_string:='SELECT AddGeometryColumn('''|| table_sheme_name||''', '''
			||table_name_db ||''','''||field_name_db||''',4326,'''|| type_geom_name ||''',2);';
		sql_string_history:= 'SELECT AddGeometryColumn('''|| table_sheme_name||''', '''
			||table_name_db||'_history'||''','''||field_name_db||''',4326,'''|| type_geom_name ||''',2);';
	END IF;
	EXECUTE sql_string;
	EXECUTE 'ALTER TABLE '||table_sheme_name||'.'||table_name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	SELECT nextval('sys_scheme.table_field_info_id_seq') INTO id_new_field;
	INSERT INTO sys_scheme.table_field_info(id, id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
	VALUES (id_new_field, table_id, field_name_db, field_name_map, field_type, true, field_name_lable, false, false);

	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_history;

	IF exists_history=TRUE THEN
		EXECUTE sql_string_history;
		EXECUTE 'ALTER TABLE '||table_sheme_name||'.'||table_name_db||'_history OWNER TO ' || sys_scheme.get_admin_group() ||';';
	END IF;

RETURN id_new_field;
END;$$;


--
-- TOC entry 3449 (class 0 OID 0)
-- Dependencies: 1032
-- Name: FUNCTION create_field(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_field(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying) IS 'Создание атрибутов таблицы';


--
-- TOC entry 1033 (class 1255 OID 36987)
-- Name: create_field_test(integer, character varying, character varying, integer, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_field_test(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
table_sheme_name character varying;
table_name_db character varying;
type_field_name character varying;
sql_string character varying;
type_geom_name character varying;
id_new_field integer;
BEGIN
	SELECT name_db INTO table_name_db FROM sys_scheme.table_info WHERE id = table_id;
	SELECT scheme_name INTO table_sheme_name FROM sys_scheme.table_info WHERE id = table_id;
	SELECT name_db INTO type_field_name FROM sys_scheme.table_type WHERE id = field_type;
	SELECT ttg.namedb INTO type_geom_name FROM sys_scheme.table_info ti, sys_scheme.table_type_geom ttg WHERE ti.id=table_id AND ttg.id = ti.geom_type;
	sql_string:='ALTER TABLE '||table_sheme_name||'.'||table_name_db||' ADD COLUMN "'||field_name_db||'" ';
	IF field_type = 1 OR field_type = 6 THEN
		sql_string:=sql_string||type_field_name||' DEFAULT 0;';
	END IF;
	IF field_type = 2 THEN
		sql_string:=sql_string||type_field_name||';';
	END IF;
	IF field_type = 3 OR field_type = 4 THEN
		sql_string:=sql_string||type_field_name||' DEFAULT now();';
	END IF;
	IF field_type = 5 THEN
		sql_string:='SELECT AddGeometryColumn('''|| table_sheme_name||''', '''
			||table_name_db ||''','''||field_name_db||''',4326,'''|| type_geom_name ||''',2);';
	END IF;
	SELECT nextval('sys_scheme.table_field_info_id_seq') INTO id_new_field;
	INSERT INTO sys_scheme.table_field_info(id, id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
	VALUES (id_new_field, table_id, field_name_db, field_name_map, field_type, true, field_name_lable, false, false);
RETURN sql_string;
END;$$;


--
-- TOC entry 3450 (class 0 OID 0)
-- Dependencies: 1033
-- Name: FUNCTION create_field_test(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_field_test(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying) IS 'Создание атрибутов таблицы';


--
-- TOC entry 1034 (class 1255 OID 36988)
-- Name: create_get_info_and_create(character varying, character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_get_info_and_create(scheme_name_db character varying, table_name_db character varying, table_name_map character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
id_table_new integer;
r information_schema.columns;
geomtype character varying;
geomtypeint integer;
BEGIN
--column_name, data_type, udt_name
SELECT "type" INTO geomtype FROM geometry_columns WHERE f_table_schema like scheme_name_db AND f_table_name like table_name_db AND f_geometry_column like 'geom';
SELECT id INTO geomtypeint FROM sys_scheme.table_type_geom WHERE (namedb like '%'||replace(geomtype, 'MULTI', '')||'%') LIMIT 1; 

id_table_new:= sys_scheme.create_exists_table(scheme_name_db, table_name_db, table_name_map, 'id', 1, geomtypeint, false, false, 0);
--raise notice '����� %', '1';
IF id_table_new>0 THEN
--raise notice '����� %', '2';
	FOR r IN SELECT * FROM information_schema.columns 
		WHERE table_schema like scheme_name_db AND table_name like table_name_db AND
		(data_type like 'integer%' OR data_type like 'character varying' OR data_type like 'numeric' OR data_type like 'date')
		AND (column_name not like 'id' AND data_type not like 'geom') LOOP
		IF r.data_type = 'integer' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 1, r.column_name);
		--END IF;
		ELSIF r.data_type = 'character varying' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 2, r.column_name);
		--END IF;
		ELSIF r.data_type = 'numeric' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 6, r.column_name);
		ELSIF r.data_type = 'date' THEN
			PERFORM sys_scheme.create_exists_field(id_table_new, r.column_name, r.column_name, 3, r.column_name);
		END IF;
		--raise notice '����� %', r.column_name;
	END LOOP;
END IF;

RETURN id_table_new;
END;$$;


--
-- TOC entry 1118 (class 1255 OID 36989)
-- Name: create_history_table(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_history_table(id_table_val integer) RETURNS integer
    LANGUAGE plpgsql
    AS $_$DECLARE
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
                NOT "substring"(t.table_name::text, 1, 1) = '_'::text AND 
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
END;$_$;


--
-- TOC entry 3451 (class 0 OID 0)
-- Dependencies: 1118
-- Name: FUNCTION create_history_table(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_history_table(id_table_val integer) IS 'Создание табилцы истории';


--
-- TOC entry 1130 (class 1255 OID 554960)
-- Name: create_history_table(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_history_table(id_table_val integer, create_photo_history boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $_$DECLARE
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
                NOT "substring"(t.table_name::text, 1, 1) = '_'::text AND 
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
END;$_$;


--
-- TOC entry 3452 (class 0 OID 0)
-- Dependencies: 1130
-- Name: FUNCTION create_history_table(id_table_val integer, create_photo_history boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_history_table(id_table_val integer, create_photo_history boolean) IS 'Создание табилцы истории';


--
-- TOC entry 1035 (class 1255 OID 36991)
-- Name: create_index(text, text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_index(scheme_name text, table_name text, field_name text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_index boolean;
is_table boolean;
BEGIN
	exists_index:=false;
	is_table:=true;
	SELECT EXISTS(SELECT true 
		  FROM pg_statio_user_indexes
		  WHERE schemaname like scheme_name AND relname like table_name AND 
indexrelname like substring(field_name||'_'||table_name||'_index' from 0 for 64)) INTO exists_index;

	SELECT sys_scheme.get_is_table(scheme_name, table_name) INTO is_table;
	IF exists_index=false AND is_table = true THEN
	EXECUTE 'CREATE INDEX "'||field_name||'_'||table_name||'_index"
		  ON "'||scheme_name||'"."'||table_name||'"
		  USING gist
		  ("'||field_name||'");';
	--EXECUTE 'VACUUM ANALYZE "'||scheme_name||'"."'||table_name||'" ("'||field_name||'");';
	RETURN true;
	END IF;
RETURN false;
END;$$;


--
-- TOC entry 1036 (class 1255 OID 36992)
-- Name: create_photo_table(text, text, text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_photo_table(scheme_name text, table_name text, table_source text, pk_field text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_photo boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like scheme_name AND tablename like table_name) INTO exists_photo;
	IF exists_photo=false THEN
		EXECUTE 'CREATE TABLE "'||scheme_name||'"."'||table_name||'"(id serial NOT NULL, '||
                                      'id_obj integer, '||
                                      'file bytea NOT NULL, '||
                                      'img_preview bytea, ' ||
                                      'file_name character varying, '||
                                      'is_photo boolean, ' ||
                                      'dataupd timestamp without time zone DEFAULT now(), '||
                                      'master_id integer, '||
				      'status integer, '||
                                      'CONSTRAINT "'||table_name||'_pkey" PRIMARY KEY ('||'id'||'), '||
                                      ' CONSTRAINT "'||table_name ||'_id_obj_fkey" FOREIGN KEY (id_obj) '||
                                      ' REFERENCES "'|| scheme_name ||'"."'|| table_source ||'" ('||pk_field||') MATCH SIMPLE '||
                                      ' ON UPDATE CASCADE ON DELETE CASCADE '||
                                    ') '||
                                    'WITH ('||
                                      'OIDS=FALSE'||
                                    ');'||
                                    'GRANT ALL ON TABLE "'|| scheme_name ||'"."'||table_name||'" TO public; 
				GRANT ALL ON TABLE "'|| scheme_name ||'"."'||table_name||'_id_seq" TO public;'||
                                    'ALTER TABLE "'|| scheme_name ||'"."'||table_name||'" OWNER TO ' || sys_scheme.get_admin_group() ||';';
		
	END IF;
	RETURN true;
END;$$;


--
-- TOC entry 3453 (class 0 OID 0)
-- Dependencies: 1036
-- Name: FUNCTION create_photo_table(scheme_name text, table_name text, table_source text, pk_field text); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_photo_table(scheme_name text, table_name text, table_source text, pk_field text) IS 'Создание таблицы с файлами';


--
-- TOC entry 1037 (class 1255 OID 36993)
-- Name: create_sync_field(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_sync_field(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
exists_field_val boolean;
exists_table_val boolean;
seq_name text;
BEGIN
	FOR rec IN SELECT id, scheme_name, name_db, pk_fileld FROM sys_scheme.table_info WHERE id = id_table_val LOOP
		SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like rec.name_db
						AND table_type like 'BASE TABLE') INTO exists_table_val;
		IF exists_table_val=true THEN
			SELECT EXISTS(SELECT true FROM information_schema.columns WHERE table_schema like rec.scheme_name 
									AND table_name like rec.name_db
									AND column_name like 'master_id') INTO exists_field_val;
			IF exists_field_val = FALSE THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' ADD COLUMN master_id integer;';
			END IF;

			SELECT EXISTS(SELECT true FROM information_schema.columns WHERE table_schema like rec.scheme_name 
									AND table_name like rec.name_db
									AND column_name like 'status') INTO exists_field_val;
			IF exists_field_val = FALSE THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' ADD COLUMN status integer;';
			END IF;
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
				FROM information_schema.columns 
				WHERE table_schema=rec.scheme_name and table_name=rec.name_db and column_name=rec.pk_fileld;
			IF seq_name<>'' AND seq_name is not null THEN
				raise notice 'SQL:: %', 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
				EXECUTE 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			END IF;	
		END IF;
	END LOOP;
	RETURN 'ok';
END;$$;


--
-- TOC entry 1039 (class 1255 OID 36994)
-- Name: create_sync_field_for_photo(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_sync_field_for_photo(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
exists_field_val boolean;
exists_table_val boolean;
seq_name text;
BEGIN
	FOR rec IN SELECT id, scheme_name, name_db FROM sys_scheme.table_info WHERE id=id_table_val LOOP
		SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like 'photo_'||rec.name_db
						AND table_type like 'BASE TABLE') INTO exists_table_val;
		IF exists_table_val=true THEN
			SELECT EXISTS(SELECT true FROM information_schema.columns WHERE table_schema like rec.scheme_name 
									AND table_name like 'photo_'||rec.name_db
									AND column_name like 'master_id') INTO exists_field_val;
			IF exists_field_val = FALSE THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.photo_'||rec.name_db||' ADD COLUMN master_id integer;';
			END IF;

			SELECT EXISTS(SELECT true FROM information_schema.columns WHERE table_schema like rec.scheme_name 
									AND table_name like 'photo_'||rec.name_db
									AND column_name like 'status') INTO exists_field_val;
			IF exists_field_val = FALSE THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.photo_'||rec.name_db||' ADD COLUMN status integer;';
			END IF;
			EXECUTE 'ALTER TABLE '||rec.scheme_name||'.photo_'||rec.name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
				FROM information_schema.columns 
				WHERE table_schema=rec.scheme_name and table_name='photo_'||rec.name_db and column_name='id';
			IF seq_name<>'' AND seq_name is not null THEN
				EXECUTE 'ALTER TABLE '||seq_name||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
			END IF;	
		END IF;
	END LOOP;
	RETURN 'ok';
END;$$;


--
-- TOC entry 1040 (class 1255 OID 36995)
-- Name: create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$BEGIN
	RETURN sys_scheme.create_table(table_scheme_name, table_name_db, table_name_sys, table_type , table_geom_type , is_style , photo_exists, table_group, false, false);
END;$$;


--
-- TOC entry 3454 (class 0 OID 0)
-- Dependencies: 1040
-- Name: FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer) IS 'Создание новой таблицы';


--
-- TOC entry 1041 (class 1255 OID 36996)
-- Name: create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $$BEGIN
RETURN sys_scheme.create_table(table_scheme_name, table_name_db, table_name_sys, table_type , table_geom_type , is_style , photo_exists, table_group, def_visible, false);
END;$$;


--
-- TOC entry 3455 (class 0 OID 0)
-- Dependencies: 1041
-- Name: FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean) IS 'Создание новой таблицы';


--
-- TOC entry 1121 (class 1255 OID 36997)
-- Name: create_table(character varying, character varying, character varying, integer, integer, boolean, boolean, integer, boolean, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_table(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group integer, def_visible boolean, is_hidden boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
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
	EXECUTE 'CREATE TABLE '||table_scheme_name||'.'||table_name_db||' (gid serial NOT NULL, master_id integer, status integer , PRIMARY KEY (gid)) WITH (OIDS = FALSE);';
ELSE
	RETURN -1;
END IF;

IF table_type = 1 THEN
	EXECUTE 'SELECT AddGeometryColumn('''|| table_scheme_name ||''', '''||table_name_db ||''',''the_geom'',4326,'''|| type_geom_name ||''',2);';
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

END;$$;


--
-- TOC entry 1123 (class 1255 OID 37620)
-- Name: create_table_with_group_name(character varying, character varying, character varying, integer, integer, boolean, boolean, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_table_with_group_name(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group_name character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$BEGIN
       RETURN sys_scheme.create_table_with_group_name(table_scheme_name,
table_name_db, table_name_sys, table_type , table_geom_type , is_style
, photo_exists, table_group_name, false, false);
END;$$;


--
-- TOC entry 1124 (class 1255 OID 37621)
-- Name: create_table_with_group_name(character varying, character varying, character varying, integer, integer, boolean, boolean, character varying, boolean, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_table_with_group_name(table_scheme_name character varying, table_name_db character varying, table_name_sys character varying, table_type integer, table_geom_type integer, is_style boolean, photo_exists boolean, table_group_name character varying, def_visible boolean, is_hidden boolean) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
id_new_table integer;
group_id_val integer;
BEGIN
SELECT sys_scheme.create_table(table_scheme_name, table_name_db,
table_name_sys, table_type, table_geom_type, is_style, photo_exists,
null, def_visible, is_hidden) INTO id_new_table;
IF id_new_table>=0 THEN
       IF table_group_name IS NOT NULL THEN
               SELECT sys_scheme.add_group(table_group_name, table_group_name) INTO group_id_val;
               INSERT INTO sys_scheme.table_groups_table (id_table, id_group) VALUES (id_new_table, group_id_val);
       END IF;
END IF;
RETURN id_new_table;

END;$$;


--
-- TOC entry 1042 (class 1255 OID 36999)
-- Name: create_user(character varying, character varying, character varying, character varying, character varying, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_user(user_login character varying, user_pass character varying, user_name_full character varying, user_otdel character varying, user_window_name character varying, user_typ integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
id_new_user integer;
BEGIN
	SELECT nextval('sys_scheme.user_db_id_seq') INTO id_new_user;
	INSERT INTO sys_scheme.user_db (id, "name", pass, name_full, "login", otdel, window_name, typ) 
		VALUES (id_new_user, '', user_pass, user_name_full, user_login, user_otdel, user_window_name, user_typ);

RETURN id_new_user;
END;$$;


--
-- TOC entry 3456 (class 0 OID 0)
-- Dependencies: 1042
-- Name: FUNCTION create_user(user_login character varying, user_pass character varying, user_name_full character varying, user_otdel character varying, user_window_name character varying, user_typ integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_user(user_login character varying, user_pass character varying, user_name_full character varying, user_otdel character varying, user_window_name character varying, user_typ integer) IS 'Создание нового пользователя';


--
-- TOC entry 1043 (class 1255 OID 37000)
-- Name: create_user(character varying, character varying, character varying, character varying, character varying, character varying, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_user(user_login character varying, user_pass character varying, user_pass_sync character varying, user_name_full character varying, user_otdel character varying, user_window_name character varying, user_typ integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$DECLARE
	id_new_user integer;
	is_admin boolean;
	rec sys_scheme.table_schems;
	exists_group boolean;
BEGIN
	IF EXISTS (SELECT true FROM pg_user WHERE usename = user_login) THEN
		RAISE EXCEPTION '|%|Такой пользователь уже есть', 10;
	ELSE
			IF user_typ=1 THEN
				is_admin = TRUE;
				SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_admin_group()) INTO exists_group;
				IF exists_group=FALSE THEN
					PERFORM sys_scheme.create_admin_group();
				END IF;
				EXECUTE 'CREATE ROLE ' || user_login || ' LOGIN ENCRYPTED PASSWORD ''' || user_pass || ''' NOSUPERUSER INHERIT CREATEDB CREATEROLE;';
				EXECUTE 'GRANT '|| sys_scheme.get_admin_group() ||' TO ' || user_login;
			END IF;
			IF user_typ=2 THEN
				is_admin = FALSE;
				EXECUTE 'CREATE ROLE ' || user_login || ' LOGIN ENCRYPTED PASSWORD ''' || user_pass || ''' NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE';
			END IF;
			--
			SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_client_group()) INTO exists_group;
			IF exists_group=FALSE THEN
				PERFORM sys_scheme.create_client_group();
			END IF;
			EXECUTE 'GRANT '|| sys_scheme.get_client_group() ||' TO ' ||  user_login;
		
			SELECT nextval('sys_scheme.user_db_id_seq') INTO id_new_user;
			INSERT INTO sys_scheme.user_db (id, "name", pass, name_full, "login", otdel,"admin", window_name, typ) 
				VALUES (id_new_user, '', user_pass_sync, user_name_full, user_login, user_otdel, is_admin, user_window_name, user_typ);
	END IF;
RETURN id_new_user;
END;$$;


--
-- TOC entry 1044 (class 1255 OID 37001)
-- Name: create_user_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_user_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	RETURN true;
END;$$;


--
-- TOC entry 1045 (class 1255 OID 37002)
-- Name: create_view_for_table(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION create_view_for_table(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
table_name_val character varying;
table_scheme_val character varying;
table_type_val integer;
table_geom_val character varying;
exists_in_gc boolean;
BEGIN

	SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;


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
RETURN table_name_val||'_vw';
END;$$;


--
-- TOC entry 3457 (class 0 OID 0)
-- Dependencies: 1045
-- Name: FUNCTION create_view_for_table(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION create_view_for_table(id_table_val integer) IS 'Создание представления';


--
-- TOC entry 1046 (class 1255 OID 37003)
-- Name: del_field(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION del_field() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
cnt integer;
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
	UPDATE sys_scheme.table_info SET view_name = sys_scheme.create_view_for_table(OLD.id_table) WHERE id = OLD.id_table;
	RETURN NEW;
END;$$;


--
-- TOC entry 1048 (class 1255 OID 37004)
-- Name: del_right_to_table(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION del_right_to_table() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
table_name character varying;
scheme_name_trg character varying;
user_name character varying;
exists_photo boolean;
pk_field character varying;
BEGIN
	SELECT ti.name_db INTO table_name FROM sys_scheme.table_info ti WHERE ti.id = OLD.id_table;
	SELECT ti.pk_fileld INTO pk_field FROM sys_scheme.table_info ti WHERE ti.id = OLD.id_table;
	SELECT ti.scheme_name INTO scheme_name_trg FROM sys_scheme.table_info ti WHERE ti.id = OLD.id_table;
	SELECT ti.photo INTO exists_photo FROM sys_scheme.table_info ti WHERE ti.id = OLD.id_table;
	SELECT ud."name" INTO user_name FROM sys_scheme.user_db ud WHERE ud.id = OLD.id_user;
	IF OLD.read_data=true AND OLD.write_data=false THEN
		EXECUTE 'REVOKE SELECT ON '||scheme_name_trg||'."'||table_name||'" FROM ' || user_name;
	END IF;
	IF (OLD.read_data=true AND OLD.write_data=true) or (OLD.read_data=false AND OLD.write_data=true) THEN
		EXECUTE 'REVOKE SELECT, UPDATE, INSERT ON '||scheme_name_trg||'."'||table_name||'" FROM ' || user_name;
	END IF;
	IF exists_photo=true THEN
		EXECUTE 'REVOKE SELECT ON '||scheme_name_trg||'."photo_'||table_name||'" FROM ' || user_name;
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."photo_'||table_name||'_'||pk_field||'_seq" FROM ' || user_name;
	END IF;
RETURN OLD;
END;$$;


--
-- TOC entry 1049 (class 1255 OID 37005)
-- Name: delete_field(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_field(field_id integer, real_delete boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
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
		EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||' DROP COLUMN '||field_namedb||';';
		EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	END IF;

	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_history;

	IF exists_history=TRUE THEN
		PERFORM sys_scheme.create_history_table(table_id);
	END IF;

	RETURN true;
END;$$;


--
-- TOC entry 3458 (class 0 OID 0)
-- Dependencies: 1049
-- Name: FUNCTION delete_field(field_id integer, real_delete boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION delete_field(field_id integer, real_delete boolean) IS 'Удаление атрибуда. Второй аргумент отвечает за удаление таблицы физически';


--
-- TOC entry 1112 (class 1255 OID 37006)
-- Name: delete_history(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_history(id_table_val integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE

table_name_val character varying;
scheme_name_val character varying;
exists_delete_tg boolean;
exists_insert_upd_tg boolean;
rec RECORD;
BEGIN
	SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT EXISTS(SELECT 1 FROM information_schema.triggers WHERE (trigger_schema like scheme_name_val) 
			AND (trigger_name like 'insert_update_'||table_name_val||'_history') LIMIT 1) INTO exists_insert_upd_tg;
	SELECT EXISTS(SELECT 1 FROM information_schema.triggers WHERE (trigger_schema like scheme_name_val) 
			AND (trigger_name like 'delete_'||table_name_val||'_history') LIMIT 1) INTO exists_delete_tg;

	--EXECUTE 'ALTER TABLE "'||scheme_name_val||'"."'||table_name_val||'" DISABLED TRIGGER';
	IF exists_insert_upd_tg=TRUE THEN
		EXECUTE 'DROP TRIGGER "insert_update_'||table_name_val||'_history" ON "'||scheme_name_val||'"."'||table_name_val||'";';
		EXECUTE 'DROP FUNCTION "'||scheme_name_val||'".trg_after_'||table_name_val||'_history();';
	END IF;
	IF exists_delete_tg=TRUE THEN
		EXECUTE 'DROP TRIGGER "delete_'||table_name_val||'_history" ON "'||scheme_name_val||'"."'||table_name_val||'";';
		EXECUTE 'DROP FUNCTION "'||scheme_name_val||'".trg_before_'||table_name_val||'_history();';
	END IF;
	FOR rec IN SELECT * FROM sys_scheme.table_history_info WHERE id_table = id_table_val LOOP
		EXECUTE 'DROP TABLE IF EXISTS "'||scheme_name_val||'"."'||rec.history_table_name||'" CASCADE;';
	END LOOP;
	DELETE FROM sys_scheme.table_history_info WHERE id_table = id_table_val;
RETURN true;
END;$$;


--
-- TOC entry 3459 (class 0 OID 0)
-- Dependencies: 1112
-- Name: FUNCTION delete_history(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION delete_history(id_table_val integer) IS 'Удаление триггеров для ведение истории';


--
-- TOC entry 1116 (class 1255 OID 37007)
-- Name: delete_table(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_table(table_id integer, real_delete boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
table_shemename character varying;
table_namedb character varying;
exists_table boolean;
exists_history boolean;
BEGIN
	SELECT scheme_name INTO table_shemename FROM sys_scheme.table_info WHERE id = table_id;
	SELECT name_db INTO table_namedb FROM sys_scheme.table_info WHERE id = table_id;
	SELECT EXISTS(SELECT 1 FROM pg_tables WHERE schemaname like table_shemename AND tablename like table_namedb) INTO exists_table;

	DELETE FROM sys_scheme.table_info WHERE id = table_id;

	IF real_delete=TRUE AND exists_table=TRUE THEN
		EXECUTE 'DROP TABLE IF EXISTS '||table_shemename||'.photo_'||table_namedb||';';
		--EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||'_vw OWNER TO '||current_user||'';
		EXECUTE 'DROP VIEW IF EXISTS  '||table_shemename||'.'||table_namedb||'_vw;';
		EXECUTE 'DROP TABLE '||table_shemename||'.'||table_namedb||';';

		SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_history;

		IF exists_history=TRUE THEN
			PERFORM  sys_scheme.delete_history(table_id);
		END IF;
	END IF;

	UPDATE sys_scheme.table_field_info SET is_reference = false, is_interval = false WHERE ref_table = table_id;
	UPDATE sys_scheme.table_info SET default_style = true 
		FROM sys_scheme.table_field_info 
		WHERE sys_scheme.table_info.id = sys_scheme.table_field_info.id_table 
			AND sys_scheme.table_field_info.ref_table = table_id
			AND sys_scheme.table_info.style_field = sys_scheme.table_field_info.name_db;
	RETURN true;
END;$$;


--
-- TOC entry 3460 (class 0 OID 0)
-- Dependencies: 1116
-- Name: FUNCTION delete_table(table_id integer, real_delete boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION delete_table(table_id integer, real_delete boolean) IS 'Удаление табицы. Второй аргумент отвечает за удаление таблицы физически';


--
-- TOC entry 1050 (class 1255 OID 37008)
-- Name: delete_table_from_group(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_table_from_group(table_id integer, group_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_item boolean;
BEGIN
	DELETE FROM sys_scheme.table_groups_table WHERE id_table = table_id AND id_group = group_id;

RETURN true;
END;$$;


--
-- TOC entry 3461 (class 0 OID 0)
-- Dependencies: 1050
-- Name: FUNCTION delete_table_from_group(table_id integer, group_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION delete_table_from_group(table_id integer, group_id integer) IS 'Удаление таблицы из группы';


--
-- TOC entry 1051 (class 1255 OID 37009)
-- Name: delete_user(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_user() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
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
EXECUTE 'DROP USER '|| login_user;
RETURN OLD;
END;$$;


--
-- TOC entry 1052 (class 1255 OID 37010)
-- Name: delete_user(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION delete_user(user_id_del integer, real_delete boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
login_user character varying;
r sys_scheme.table_right;
BEGIN
	SELECT "login" INTO login_user FROM sys_scheme.user_db WHERE id = user_id_del;
BEGIN
	FOR r IN SELECT * FROM sys_scheme.table_right WHERE id_user = user_id_del AND (read_data=TRUE OR write_data = TRUE) LOOP
		PERFORM sys_scheme.set_right(r.id_table, r.id_user, false, false);
	END LOOP;
END;	
	DELETE FROM sys_scheme.user_db WHERE id = user_id_del;
	
	IF real_delete = TRUE THEN
		--EXECUTE 'DROP ROLE '|| login_user;
	END IF;
RETURN true;
END;$$;


--
-- TOC entry 3462 (class 0 OID 0)
-- Dependencies: 1052
-- Name: FUNCTION delete_user(user_id_del integer, real_delete boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION delete_user(user_id_del integer, real_delete boolean) IS 'Удаление пользователя. Второй аргумент отвечает за физическое удаление';


--
-- TOC entry 1053 (class 1255 OID 37011)
-- Name: fix_admin_user_for_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_admin_user_for_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
BEGIN
	FOR rec in SELECT id, "login" FROM sys_scheme.user_db WHERE typ=1 LOOP
		BEGIN
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO '||rec."login"||' ;';
			EXECUTE 'GRANT '||sys_scheme.get_admin_group()||' TO '||rec."login"||' ;';
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1133 (class 1255 OID 37012)
-- Name: fix_client_user_for_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_client_user_for_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
BEGIN
	FOR rec in SELECT id, "login" FROM sys_scheme.user_db LOOP
		BEGIN
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO '||rec."login"||';';
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1131 (class 1255 OID 37013)
-- Name: fix_geom_indexes(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_geom_indexes() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
BEGIN
	FOR rec in SELECT ti.scheme_name, ti.name_db as table_name, tfi.name_db as field_name
			  FROM sys_scheme.table_field_info tfi, sys_scheme.table_info ti
			  WHERE type_field=5 AND ti.id = tfi.id_table LOOP
		BEGIN
			PERFORM sys_scheme.create_index(rec.scheme_name, rec.table_name, rec.field_name);
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1063 (class 1255 OID 37014)
-- Name: fix_geomtry_colums(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_geomtry_colums() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
rec_srid RECORD;
exists_in_gc boolean;
type_geom_in_sys text;
exists_obj boolean;
BEGIN
	FOR rec in SELECT id, scheme_name, name_db, geom_type, geom_field FROM sys_scheme.table_info WHERE "type"=1 LOOP
		BEGIN
			SELECT EXISTS(SELECT TRUE FROM geometry_columns WHERE f_table_schema like rec.scheme_name AND f_table_name like rec.name_db 
						AND f_geometry_column like rec.geom_field) INTO exists_in_gc;
			exists_obj:=false;
			IF exists_in_gc=FALSE THEN
				SELECT namedb INTO type_geom_in_sys FROM sys_scheme.table_type_geom WHERE id =rec.geom_type; 
				FOR rec_srid IN EXECUTE 'SELECT srid('||rec.geom_field||')::INTEGER as srid_val, geometrytype('||rec.geom_field||')::text as geom_type
							FROM "'||rec.scheme_name||'"."'||rec.name_db||'" LIMIT 1' LOOP

					exists_obj:=true;
					IF rec_srid.srid_val is not null THEN
						INSERT INTO geometry_columns(
							    f_table_catalog, f_table_schema, f_table_name, f_geometry_column, 
							    coord_dimension, srid, "type")
						    VALUES ('', rec.scheme_name, rec.name_db, rec.geom_field, 
							    2, rec_srid.srid_val, rec_srid.geom_type);
					ELSE
						INSERT INTO geometry_columns(
							    f_table_catalog, f_table_schema, f_table_name, f_geometry_column, 
							    coord_dimension, srid, "type")
						    VALUES ('', rec.scheme_name, rec.name_db, rec.geom_field, 
							    2, 4326, type_geom_in_sys);
					END IF;
				END LOOP;
				if exists_obj=false THEN
					raise notice '3:: %', 'нет объекта';
					INSERT INTO geometry_columns(
							    f_table_catalog, f_table_schema, f_table_name, f_geometry_column, 
							    coord_dimension, srid, "type")
						    VALUES ('', rec.scheme_name, rec.name_db, rec.geom_field, 
							    2, 4326, type_geom_in_sys);
				END IF;
			END IF;
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1054 (class 1255 OID 37015)
-- Name: fix_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	PERFORM sys_scheme.create_admin_group();
	PERFORM sys_scheme.create_client_group();
	PERFORM sys_scheme.create_user_groups();
RETURN true;
END;$$;


--
-- TOC entry 1062 (class 1255 OID 37016)
-- Name: fix_sequence_numbers(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_sequence_numbers() RETURNS boolean
    LANGUAGE plpgsql
    AS $$
DECLARE
 seq record;
BEGIN
 FOR seq IN (SELECT table_schema AS scheme_name,table_name,column_name,pg_get_serial_sequence(table_schema||'.'||table_name, column_name) AS seq_name
   FROM sys_scheme.table_info ti LEFT JOIN information_schema.columns col
    ON col.table_schema=ti.scheme_name AND col.table_name=ti.name_db AND ti.pk_fileld=column_name
   WHERE pg_get_serial_sequence(table_schema||'.'||table_name, column_name) IS NOT NULL) 
  LOOP
	BEGIN
		EXECUTE 'select case when (select max('||seq.column_name||') from '||seq.scheme_name||'.'||seq.table_name||
			')>(select last_value from '||seq.seq_name||') then setval('''||seq.seq_name||
			''', (select max('||seq.column_name||') from '||seq.scheme_name||'.'||seq.table_name||')) else 1 end';
	EXCEPTION
		WHEN OTHERS THEN
		RAISE NOTICE 'ERROR: %', SQLERRM;
	END;	
  END LOOP;
 RETURN true;
END;
$$;


--
-- TOC entry 1134 (class 1255 OID 37017)
-- Name: fix_sys_scheme(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_sys_scheme() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
rec2 RECORD;
exists_scheme boolean;
dubl_item integer;
BEGIN
	FOR rec IN (SELECT scheme_name FROM sys_scheme.table_info GROUP BY scheme_name) LOOP
		SELECT exists(SELECT true FROM sys_scheme.table_schems WHERE "name" like rec.scheme_name) INTO exists_scheme;
		IF exists_scheme=false THEN
			INSERT INTO sys_scheme.table_schems ("name") VALUES (rec.scheme_name);
		END IF;
	END LOOP;
	FOR rec IN SELECT "name", count("name") as cnt FROM sys_scheme.table_schems GROUP BY "name" LOOP
		IF rec.cnt>1 THEN
			SELECT id INTO dubl_item FROM sys_scheme.table_schems WHERE "name" like rec."name" limit 1;
			DELETE FROM sys_scheme.table_schems WHERE "name" like rec."name" AND id<>dubl_item;
		END IF;
	END LOOP;
	SELECT exists(SELECT true FROM information_schema.constraint_column_usage WHERE table_schema like 'sys_scheme'
							AND table_name like 'table_schems' AND column_name like 'name') INTO exists_scheme;
	IF exists_scheme=false THEN
		ALTER TABLE sys_scheme.table_schems ADD UNIQUE ("name");
	END IF;

	FOR rec IN SELECT "name" FROM sys_scheme.table_schems LOOP
		BEGIN
			EXECUTE 'GRANT ALL ON SCHEMA '||rec."name"||' TO '||sys_scheme.get_admin_group()||';';
			EXECUTE 'GRANT USAGE ON SCHEMA '||rec."name"||' TO '||sys_scheme.get_client_group()||';';
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1060 (class 1255 OID 37018)
-- Name: fix_table_grants(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_table_grants() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
u_rec record;
r_rec record;
full_name_table text;
name_table text;
sceme_name_table text;
exists_photo boolean;
name_table_photo text;
pk_field text;
seq_name text;
BEGIN
	FOR u_rec IN SELECT id, "login", typ FROM sys_scheme.user_db LOOP
		BEGIN
			FOR r_rec IN SELECT id_table, read_data, write_data FROM sys_scheme.table_right WHERE id_user = u_rec.id LOOP
				SELECT '"'||scheme_name||'"."'||name_db||'"', scheme_name, name_db, pk_fileld INTO full_name_table, sceme_name_table, name_table, pk_field
								FROM sys_scheme.table_info WHERE id=r_rec.id_table;
				SELECT substring(column_default, '.*''(.*)''.*') INTO seq_name
					FROM information_schema.columns  
					WHERE table_schema=sceme_name_table and table_name=name_table and column_name=pk_field;
	raise notice 'SEQ:: %', seq_name;
				SELECT exists(SELECT true FROM sys_scheme.table_photo_info WHERE id_table = r_rec.id_table) INTO exists_photo;
				IF exists_photo THEN
					SELECT photo_table INTO name_table_photo FROM sys_scheme.table_photo_info WHERE id_table = r_rec.id_table;
				END IF;
				IF r_rec.read_data=true AND r_rec.write_data=false THEN			
					EXECUTE 'REVOKE ALL ON '||full_name_table||' FROM ' || u_rec."login";
					EXECUTE 'GRANT SELECT ON '||full_name_table||' TO ' || u_rec."login";
					IF exists_photo=true THEN
						EXECUTE 'GRANT SELECT ON "'||sceme_name_table||'"."'||name_table_photo||'" TO ' || u_rec."login";
						EXECUTE 'GRANT ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" TO ' || u_rec."login";
					END IF;
				END IF;
				IF (r_rec.read_data=true AND r_rec.write_data=true) or (r_rec.read_data=false AND r_rec.write_data=true) THEN
					EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||full_name_table||' TO ' || u_rec."login";
	raise notice 'GRANT:: %', 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||full_name_table||' TO ' || u_rec."login";
					IF seq_name is not null AND seq_name not like '' THEN
						EXECUTE 'GRANT ALL ON '||seq_name||' TO ' || u_rec."login";
					END IF;
					IF exists_photo=true THEN
						EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON "'||sceme_name_table||'"."'||name_table_photo||'" TO ' || u_rec."login";
						EXECUTE 'GRANT ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" TO ' || u_rec."login";
					END IF;
				END IF;
				IF (r_rec.read_data=false AND r_rec.write_data=false) THEN
					EXECUTE 'REVOKE ALL ON '||full_name_table||' FROM ' || u_rec."login";
					IF seq_name is not null AND seq_name not like '' THEN
						EXECUTE 'REVOKE ALL ON '||seq_name||' FROM ' || u_rec."login";
					END IF;
					IF exists_photo=true THEN
						EXECUTE 'REVOKE SELECT, UPDATE, INSERT, DELETE ON "'||sceme_name_table||'"."'||name_table_photo||'" FROM ' || u_rec."login";
						EXECUTE 'REVOKE ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" FROM ' || u_rec."login";
					END IF;
				END IF;
			END LOOP;
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1061 (class 1255 OID 37019)
-- Name: fix_table_owner(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_table_owner() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
rec RECORD;
rec2 RECORD;
seq_name text;
exists_table_val boolean;
BEGIN
	FOR rec IN SELECT id, scheme_name, name_db, pk_fileld FROM sys_scheme.table_info LOOP
		BEGIN
			SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
							AND table_name like rec.name_db
							AND table_type like 'BASE TABLE') INTO exists_table_val;
			IF exists_table_val=true THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
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
			END IF;
			END LOOP;
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;

	FOR rec IN SELECT id, scheme_name, name_db FROM sys_scheme.table_info LOOP
		BEGIN
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
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1055 (class 1255 OID 37020)
-- Name: fix_user_for_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_user_for_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	PERFORM sys_scheme.fix_admin_user_for_groups();
	PERFORM sys_scheme.fix_client_user_for_groups();
	PERFORM sys_scheme.fix_user_for_user_groups();
RETURN true;
END;$$;


--
-- TOC entry 1056 (class 1255 OID 37021)
-- Name: fix_user_for_user_groups(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION fix_user_for_user_groups() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
RETURN true;
END;$$;


--
-- TOC entry 1057 (class 1255 OID 37022)
-- Name: get_admin_group(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_admin_group() RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	return 'admin_'||current_database();
END;$$;


--
-- TOC entry 3463 (class 0 OID 0)
-- Dependencies: 1057
-- Name: FUNCTION get_admin_group(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_admin_group() IS 'Получить название группы администраторов';


--
-- TOC entry 1058 (class 1255 OID 37023)
-- Name: get_client_group(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_client_group() RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	return 'client_'||current_database();
END;$$;


--
-- TOC entry 3464 (class 0 OID 0)
-- Dependencies: 1058
-- Name: FUNCTION get_client_group(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_client_group() IS 'Получить название группы всех пользователей';


--
-- TOC entry 1113 (class 1255 OID 37024)
-- Name: get_group_list(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_group_list() RETURNS SETOF group_info
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
BEGIN
 FOR r IN SELECT id, name_group, descript, order_num FROM sys_scheme.table_groups LOOP
  RETURN NEXT r;
 END LOOP;
 RETURN;
END;
$$;


--
-- TOC entry 3465 (class 0 OID 0)
-- Dependencies: 1113
-- Name: FUNCTION get_group_list(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_group_list() IS '��������� ������ ����';


--
-- TOC entry 1114 (class 1255 OID 37025)
-- Name: get_group_list_for_table(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_group_list_for_table(table_id integer) RETURNS SETOF group_info
    LANGUAGE plpgsql
    AS $$
DECLARE
r RECORD;
BEGIN
 FOR r IN (SELECT tg.* 
   FROM sys_scheme.table_groups tg, sys_scheme.table_groups_table tgt
   WHERE tgt.id_table = table_id AND tg.id = tgt.id_group) LOOP
  RETURN NEXT r;
 END LOOP;
 RETURN;
END;
$$;


--
-- TOC entry 3466 (class 0 OID 0)
-- Dependencies: 1114
-- Name: FUNCTION get_group_list_for_table(table_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_group_list_for_table(table_id integer) IS '��������� ������ ���� � ������� ������� �������';


SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 170 (class 1259 OID 37026)
-- Name: table_groups; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_groups (
    id integer NOT NULL,
    name_group character varying,
    descript character varying,
    order_num integer
);


--
-- TOC entry 1059 (class 1255 OID 37032)
-- Name: get_group_list_no_for_table(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_group_list_no_for_table(table_id integer) RETURNS SETOF table_groups
    LANGUAGE plpgsql
    AS $$
DECLARE
	r RECORD;
BEGIN
	FOR r IN (SELECT * FROM
                    (SELECT t.id,t.name_group,t.descript, t.order_num FROM sys_scheme.table_groups AS t
                     EXCEPT 
                    SELECT DISTINCT t.id,t.name_group,t.descript, t.order_num FROM sys_scheme.table_groups_table AS g,sys_scheme .table_groups AS t
                     WHERE g.id_table=table_id AND g.id_group=t.id
                    )AS al ORDER BY id) LOOP
		RETURN NEXT r;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3467 (class 0 OID 0)
-- Dependencies: 1059
-- Name: FUNCTION get_group_list_no_for_table(table_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_group_list_no_for_table(table_id integer) IS 'Получение списка груп в которых не состоит таблица';


--
-- TOC entry 1115 (class 1255 OID 37033)
-- Name: get_group_list_without_null(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_group_list_without_null() RETURNS SETOF group_info
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
exists_in_group boolean;
BEGIN
 exists_in_group:=false;
 FOR r IN SELECT id, name_group, descript, order_num FROM sys_scheme.table_groups LOOP
  SELECT exists(SELECT true FROM sys_scheme.table_groups_table tgt, sys_scheme.table_right tr, sys_scheme.user_db udb
   WHERE tr.id_table = tgt.id_table AND udb."login" like current_user AND tr.id_user = udb.id 
   AND (tr.read_data = true OR tr.write_data = true) 
    AND tgt.id_group = r.id) INTO exists_in_group;
  IF exists_in_group = TRUE THEN
   RETURN NEXT r;
  END IF;
 exists_in_group:=false;
 END LOOP;
 RETURN;
END;
$$;


--
-- TOC entry 3468 (class 0 OID 0)
-- Dependencies: 1115
-- Name: FUNCTION get_group_list_without_null(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_group_list_without_null() IS '��������� ������ �� ������ ����';


--
-- TOC entry 1064 (class 1255 OID 37034)
-- Name: get_history_info(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_info(table_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_item;
RETURN exists_item;
END;$$;


--
-- TOC entry 3469 (class 0 OID 0)
-- Dependencies: 1064
-- Name: FUNCTION get_history_info(table_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_history_info(table_id integer) IS 'Включена ли история';


--
-- TOC entry 1120 (class 1255 OID 68580)
-- Name: get_history_list_by_object(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_list_by_object() RETURNS SETOF history_list_by_user
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
sql_string character varying;
rec sys_scheme.history_list_by_user;
BEGIN
	sql_string:=sys_scheme.get_sql_history_list_by_user();
	IF sql_string ='' OR sql_string is null THEN
		RETURN;
	END IF;
	FOR r IN EXECUTE sql_string LOOP
		rec.user_name:=r.user_name;
		rec.data_changes:=r.dataupd;
		rec.id_history:=r.id_history;
		rec.type_operation:=r.type_operation;
		rec.id_history_table:= r.id_history_table;
		rec.id_table:= r.id_table;
		rec.id_object:=r.id;
		--raise notice 'Пункт %', rec;
		RETURN NEXT rec;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 1119 (class 1255 OID 68579)
-- Name: get_history_list_by_object(character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_list_by_object(user_name_val character varying) RETURNS SETOF history_list_by_user
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
sql_string character varying;
rec sys_scheme.history_list_by_user;
BEGIN
	sql_string:=sys_scheme.get_sql_history_list_by_user(user_name_val);
	IF sql_string ='' OR sql_string is null THEN
		RETURN;
	END IF;
	FOR r IN EXECUTE sql_string LOOP
		rec.user_name:=r.user_name;
		rec.data_changes:=r.dataupd;
		rec.id_history:=r.id_history;
		rec.type_operation:=r.type_operation;
		rec.id_history_table:= r.id_history_table;
		rec.id_table:= r.id_table;
		rec.id_object:=r.id;
		--raise notice 'Пункт %', rec;
		RETURN NEXT rec;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 1125 (class 1255 OID 68581)
-- Name: get_history_list_by_object(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_list_by_object(id_table_val integer) RETURNS SETOF history_list_by_user
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
sql_string character varying;
rec sys_scheme.history_list_by_user;
BEGIN
	sql_string:=sys_scheme.get_sql_history_list_by_user(id_table_val);
	IF sql_string ='' OR sql_string is null THEN
		RETURN;
	END IF;
	FOR r IN EXECUTE sql_string LOOP
		rec.user_name:=r.user_name;
		rec.data_changes:=r.dataupd;
		rec.id_history:=r.id_history;
		rec.type_operation:=r.type_operation;
		rec.id_history_table:= r.id_history_table;
		rec.id_table:= r.id_table;
		rec.id_object:=r.id;
		--raise notice 'Пункт %', rec;
		RETURN NEXT rec;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 1126 (class 1255 OID 68582)
-- Name: get_history_list_by_object(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_list_by_object(id_table_val integer, id_obj_val integer) RETURNS SETOF history_list_by_user
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
sql_string character varying;
rec sys_scheme.history_list_by_user;
BEGIN
	sql_string:=sys_scheme.get_sql_history_list_by_user(id_table_val, id_obj_val);
	IF sql_string ='' OR sql_string is null THEN
		RETURN;
	END IF;
	FOR r IN EXECUTE sql_string LOOP
		rec.user_name:=r.user_name;
		rec.data_changes:=r.dataupd;
		rec.id_history:=r.id_history;
		rec.type_operation:=r.type_operation;
		rec.id_history_table:= r.id_history_table;
		rec.id_table:= r.id_table;
		rec.id_object:=r.id;
		--raise notice 'Пункт %', rec;
		RETURN NEXT rec;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 1065 (class 1255 OID 37039)
-- Name: get_history_object(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_history_object(id_history_table_val integer, id_obj_history_val integer) RETURNS SETOF atribut_info
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
rec sys_scheme.atribut_info;
fi sys_scheme.table_field_info;
history_table_name_val character varying;
history_table_scheme_name_val character varying;
history_field_name_val character varying;
id_table_val integer;
id_field_val integer;
sql_string character varying;
exists_field boolean;
BEGIN
	SELECT history_table_name INTO history_table_name_val FROM sys_scheme.table_history_info WHERE id_history_table=id_history_table_val;
	SELECT id_table INTO id_table_val FROM sys_scheme.table_history_info WHERE id_history_table = id_history_table_val;
	SELECT scheme_name INTO history_table_scheme_name_val FROM sys_scheme.table_info WHERE id=id_table_val;
	sql_string:='FROM "'||history_table_scheme_name_val||'"."'||history_table_name_val||'" WHERE "id_history"='||id_obj_history_val;
	FOR fi IN (SELECT * FROM sys_scheme.table_field_info WHERE id_table = id_table_val) LOOP
		IF fi.type_field<>5 THEN
			SELECT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_schema like history_table_scheme_name_val
						AND table_name like history_table_name_val AND column_name like fi.name_db) INTO exists_field;
			IF exists_field=TRUE THEN
				FOR r IN EXECUTE 'SELECT "'||fi.name_db||'":: text as val '||sql_string LOOP
					rec.field_name = fi.name_db;
					rec.field_sys_name = fi.name_map;
					rec.field_val = r.val;
					rec.data_type = fi.type_field;
					RETURN NEXT rec;
				END LOOP;
			END IF;
			exists_field=TRUE;
		END IF;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3470 (class 0 OID 0)
-- Dependencies: 1065
-- Name: FUNCTION get_history_object(id_history_table_val integer, id_obj_history_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_history_object(id_history_table_val integer, id_obj_history_val integer) IS 'Получение информации о изменении';


--
-- TOC entry 1066 (class 1255 OID 37040)
-- Name: get_is_table(text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_is_table(scheme_name_val text, table_name_val text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
is_table boolean;
BEGIN
	is_table:=true;
	SELECT EXISTS(SELECT true FROM information_schema.tables 
			WHERE table_schema like scheme_name_val AND table_name 
			like table_name_val AND table_type like 'BASE TABLE') INTO is_table;
RETURN is_table;
END;$$;


--
-- TOC entry 1067 (class 1255 OID 37041)
-- Name: get_sql_create_view(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_create_view(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $_$DECLARE
table_name_val character varying;
scheme_name_val character varying;

index_tab_alias integer;
alias_string character varying;
alias_string2 character varying;
sql_string character varying;
select_string character varying;
from_string character varying;

rec_field sys_scheme.table_field_info;

BEGIN
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
from_string:='FROM "'||scheme_name_val||'"."'||table_name_val||'" ';
--raise notice '1:: %', from_string;
FOR rec_field IN (SELECT id, id_table, name_db, name_map, type_field, visible, name_lable, 
			       is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, 
			       ref_field_name, num_order
			  FROM sys_scheme.table_field_info
			  WHERE id_table = id_table_val AND visible = TRUE ORDER BY num_order) LOOP
--raise notice '2:: %', select_string;
	IF rec_field.is_reference=FALSE AND rec_field.is_interval = FALSE THEN
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as '||rec_field.name_db||', ';
	ELSIF rec_field.is_reference=TRUE THEN
		alias_string:='___v$'||index_tab_alias;
		select_string:=select_string||alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"
				as '||rec_field.name_db||', ';

		from_string:= from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string||' ON "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"='||
							alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" ';
	index_tab_alias:=index_tab_alias+1;
	ELSIF rec_field.is_interval=TRUE THEN
		alias_string:='___v$'||index_tab_alias;
		index_tab_alias:=index_tab_alias+1;
		alias_string2:='___v$'||index_tab_alias;
		select_string:=select_string || alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"||''(''||"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"||'')'' '||
		'as '||rec_field.name_db||', ';

		from_string:=from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string||' ON '||alias_string||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" = (SELECT '||alias_string2||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" FROM "'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string2||' WHERE "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'">'||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" AND 
					"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"<='||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field_end)||'" LIMIT 1) ';
	index_tab_alias:=index_tab_alias+1;
	END IF;
--raise notice 'SELECT:: %', select_string;
--raise notice 'FROM:: %', from_string;
	
END LOOP;
--raise notice 'SELECT_STOP:: %', select_string;
select_string:=substring(select_string from 0 for char_length(select_string)-1);
sql_string:=select_string||' '||from_string;
RETURN sql_string;
END;$_$;


--
-- TOC entry 3471 (class 0 OID 0)
-- Dependencies: 1067
-- Name: FUNCTION get_sql_create_view(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_create_view(id_table_val integer) IS 'Получение sql-запроса для создания Представления';


--
-- TOC entry 1128 (class 1255 OID 37042)
-- Name: get_sql_for_table(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_for_table(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $_$DECLARE
table_name_val character varying;
scheme_name_val character varying;

index_tab_alias integer;
alias_string character varying;
alias_string2 character varying;
sql_string character varying;
select_string character varying;
select_id_string character varying;
from_string character varying;

rec_field sys_scheme.table_field_info;

BEGIN
select_id_string:=', ';
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
from_string:='FROM "'||scheme_name_val||'"."'||table_name_val||'" ';
FOR rec_field IN (SELECT id, id_table, name_db, name_map, type_field, visible, name_lable, 
                   is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, 
                   ref_field_name, num_order
              FROM sys_scheme.table_field_info
              WHERE id_table = id_table_val AND visible = TRUE AND type_field<>5 ORDER BY num_order) LOOP
    IF rec_field.is_reference=FALSE AND rec_field.is_interval = FALSE THEN
        select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
                        'as '||rec_field.name_db||', ';
    ELSIF rec_field.is_reference=TRUE THEN
        select_id_string:=select_id_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
                        'as "id!'||rec_field.name_db||'", ';
        alias_string:='___v$'||index_tab_alias;
        select_string:=select_string||alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'" 
                as '||rec_field.name_db||', ';

        from_string:= from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
                    '"."'||sys_scheme.get_table_name(rec_field.ref_table)||
                    '" '||alias_string||' ON "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"='||
                            alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" ';
    index_tab_alias:=index_tab_alias+1;
    ELSIF rec_field.is_interval=TRUE THEN
        select_id_string:=select_id_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
                        'as "id!'||rec_field.name_db||'", ';
        alias_string:='___v$'||index_tab_alias;
        index_tab_alias:=index_tab_alias+1;
        alias_string2:='___v$'||index_tab_alias;
        select_string:=select_string ||'COALESCE('||alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"||''(''||"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"||'')'',"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"::text) 
        as '||rec_field.name_db||', ';

        from_string:=from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
                    '"."'||sys_scheme.get_table_name(rec_field.ref_table)||
                    '" '||alias_string||' ON '||alias_string||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
                    '" = (SELECT '||alias_string2||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
                    '" FROM "'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
                    '"."'||sys_scheme.get_table_name(rec_field.ref_table)||
                    '" '||alias_string2||' WHERE "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'">'||alias_string2||
                    '."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" AND 
                    "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"<='||alias_string2||
                    '."'||sys_scheme.get_table_field_name(rec_field.ref_field_end)||'" LIMIT 1) ';
    index_tab_alias:=index_tab_alias+1;
    END IF;
--raise notice 'SELECT:: %', select_string;
--raise notice 'FROM:: %', from_string;

END LOOP;
IF (char_length(select_id_string)>=2) THEN
    select_string:=substring(select_string from 0 for char_length(select_string)-1);
END IF;
select_id_string:=substring(select_id_string from 0 for char_length(select_id_string)-1);

sql_string:=select_string||select_id_string||' '||from_string;
RETURN sql_string;
END;$_$;


--
-- TOC entry 3472 (class 0 OID 0)
-- Dependencies: 1128
-- Name: FUNCTION get_sql_for_table(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_for_table(id_table_val integer) IS 'Получение sql-запроса для создания Представления';


--
-- TOC entry 1069 (class 1255 OID 37043)
-- Name: get_sql_for_table_new(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_for_table_new(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $_$DECLARE
table_name_val character varying;
scheme_name_val character varying;

index_tab_alias integer;
alias_string character varying;
alias_string2 character varying;
sql_string character varying;
select_string character varying;
from_string character varying;
ref_table_type integer;
rec_field sys_scheme.table_field_info;

BEGIN
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
from_string:='FROM "'||scheme_name_val||'"."'||table_name_val||'" ';
FOR rec_field IN (SELECT id, id_table, name_db, name_map, type_field, visible, name_lable, 
			       is_reference, is_interval, is_style, ref_table, ref_field, ref_field_end, 
			       ref_field_name, num_order
			  FROM sys_scheme.table_field_info
			  WHERE id_table = id_table_val AND visible = TRUE AND type_field<>5 ORDER BY num_order) LOOP
	IF rec_field.is_reference=FALSE AND rec_field.is_interval = FALSE THEN
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as '||rec_field.name_db||', ';
	ELSIF rec_field.is_reference=TRUE THEN
		SELECT "type" INTO ref_table_type FROM sys_scheme.table_info WHERE id = rec_field.ref_table;
		IF ref_table_type=1 OR ref_table_type=4 THEN
			select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as '||rec_field.name_db||', ';
		ELSE
			alias_string:='___v$'||index_tab_alias;
			select_string:=select_string||alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"
					as '||rec_field.name_db||', ';

			from_string:= from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
						'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
						'" '||alias_string||' ON "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"='||
								alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" ';
			index_tab_alias:=index_tab_alias+1;
		END IF;
	ELSIF rec_field.is_interval=TRUE THEN
		alias_string:='___v$'||index_tab_alias;
		index_tab_alias:=index_tab_alias+1;
		alias_string2:='___v$'||index_tab_alias;
		select_string:=select_string || alias_string||'."'||sys_scheme.get_table_field_name(rec_field.ref_field_name)||'"||''(''||"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"||'')'' '||
		'as '||rec_field.name_db||', ';

		from_string:=from_string|| 'LEFT JOIN '||'"'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string||' ON '||alias_string||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" = (SELECT '||alias_string2||'."'||sys_scheme.get_table_pkfield(rec_field.ref_table)||
					'" FROM "'||sys_scheme.get_table_scheme_name(rec_field.ref_table)||
					'"."'||sys_scheme.get_table_name(rec_field.ref_table)||
					'" '||alias_string2||' WHERE "'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'">'||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field)||'" AND 
					"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'"<='||alias_string2||
					'."'||sys_scheme.get_table_field_name(rec_field.ref_field_end)||'" LIMIT 1) ';
	index_tab_alias:=index_tab_alias+1;
	END IF;
--raise notice 'SELECT:: %', select_string;
--raise notice 'FROM:: %', from_string;
	
END LOOP;
select_string:=substring(select_string from 0 for char_length(select_string)-1);
sql_string:=select_string||' '||from_string;
RETURN sql_string;
END;$_$;


--
-- TOC entry 3473 (class 0 OID 0)
-- Dependencies: 1069
-- Name: FUNCTION get_sql_for_table_new(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_for_table_new(id_table_val integer) IS 'Получение sql-запроса для создания Представления';


--
-- TOC entry 1070 (class 1255 OID 37044)
-- Name: get_sql_for_view(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_for_view(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
	table_name_val character varying;
	scheme_name_val character varying;
	sql_string character varying;
	select_string character varying;
	rec RECORD;
BEGIN
	sql_string:='';
	select_string:='SELECT ';

	SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT view_name INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	FOR rec IN (SELECT column_name, ordinal_position, udt_name FROM information_schema.columns 
			WHERE table_schema like scheme_name_val AND table_name like table_name_val ORDER BY ordinal_position) LOOP
		IF rec.udt_name<>'geometry' THEN
			select_string:=select_string||'"'||rec.column_name||'", ';
		END IF;
	END LOOP;
	select_string:=substring(select_string from 0 for char_length(select_string)-1);
	select_string:=select_string||' FROM "'||scheme_name_val||'"."'||table_name_val||'"';
	RETURN select_string;
END;$$;


--
-- TOC entry 3474 (class 0 OID 0)
-- Dependencies: 1070
-- Name: FUNCTION get_sql_for_view(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_for_view(id_table_val integer) IS 'Получение sql-запроса для SELECT без геометрии из Представления';


--
-- TOC entry 1071 (class 1255 OID 37045)
-- Name: get_sql_history_list_by_user(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_history_list_by_user() RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
history_info sys_scheme.table_history_info;
table_scheme_val character varying;
sql_string character varying;
from_string character varying;
where_string character varying;
pk_field_val character varying;
count_history_table integer;
id_table_val integer;
i integer;
BEGIN
	sql_string:='SELECT b.id_history, b.dataupd, b.type_operation, b.user_name, b.id, b.id_history_table, b.id_table FROM ';
	from_string:='';
	where_string:='';
	count_history_table:=0;
	SELECT count(*) INTO count_history_table FROM sys_scheme.table_history_info;
	i:=1;
	FOR id_table_val in SELECT id_table FROM sys_scheme.table_history_info GROUP BY id_table LOOP
		
		SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
		SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;	
		IF count_history_table>0 THEN
			FOR history_info IN (SELECT * FROM sys_scheme.table_history_info 
						WHERE id_table = id_table_val ORDER BY id_history_table) LOOP
				from_string:= from_string||' SELECT "'||table_scheme_val||'"."'||history_info.history_table_name||'"."id_history", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."dataupd", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."type_operation", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."user_name",
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."'||pk_field_val||'" as id,
								'||history_info.id_history_table||' as id_history_table,
								'||history_info.id_table||' as id_table
								FROM "'||table_scheme_val||'"."'||history_info.history_table_name||'" ';
				if count_history_table>i THEN
					from_string:= from_string||'UNION';
				END IF;
				i:=i+1;
			END LOOP;
		ELSE
			RETURN '';
		END IF;
	END LOOP;
	where_string:=';';
	sql_string:=sql_string ||'('||from_string||') b '||where_string;
	RETURN sql_string;
END;$$;


--
-- TOC entry 3475 (class 0 OID 0)
-- Dependencies: 1071
-- Name: FUNCTION get_sql_history_list_by_user(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_history_list_by_user() IS 'Получение sql-запроса о изменении объекта в виде :пользователь дата и время изменения:';


--
-- TOC entry 1072 (class 1255 OID 37046)
-- Name: get_sql_history_list_by_user(character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_history_list_by_user(user_name_val character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
history_info sys_scheme.table_history_info;
table_scheme_val character varying;
sql_string character varying;
from_string character varying;
where_string character varying;
pk_field_val character varying;
count_history_table integer;
id_table_val integer;
i integer;
BEGIN
	sql_string:='SELECT b.id_history, b.dataupd, b.type_operation, b.user_name, b.id, b.id_history_table, b.id_table FROM ';
	from_string:='';
	where_string:='';
	count_history_table:=0;
	SELECT count(*) INTO count_history_table FROM sys_scheme.table_history_info;
	i:=1;
	FOR id_table_val in SELECT id_table FROM sys_scheme.table_history_info GROUP BY id_table LOOP
		
		SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
		SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;
		
		--raise notice '1 Зашел %', id_table_val;
		
		IF count_history_table>0 THEN
			--raise notice '2 Зашел %', count_history_table;
			FOR history_info IN (SELECT * FROM sys_scheme.table_history_info 
						WHERE id_table = id_table_val ORDER BY id_history_table) LOOP
				--raise notice '3 Зашел %', history_info;
				from_string:= from_string||' SELECT "'||table_scheme_val||'"."'||history_info.history_table_name||'"."id_history", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."dataupd", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."type_operation", 
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."user_name",
								"'||table_scheme_val||'"."'||history_info.history_table_name||'"."'||pk_field_val||'" as id,
								'||history_info.id_history_table||' as id_history_table,
								'||history_info.id_table||' as id_table
								FROM "'||table_scheme_val||'"."'||history_info.history_table_name||'" ';
				if count_history_table>i THEN
					from_string:= from_string||'UNION';
				END IF;
				--raise notice '4 Зашел %', from_string;
				i:=i+1;
			END LOOP;
		ELSE
			RETURN '';
		END IF;
	END LOOP;
	where_string:='WHERE b.user_name = '''||user_name_val||''';';
	sql_string:=sql_string ||'('||from_string||') b '||where_string;
	--raise notice '5 Зашел %', sql_string;
	RETURN sql_string;
END;$$;


--
-- TOC entry 3476 (class 0 OID 0)
-- Dependencies: 1072
-- Name: FUNCTION get_sql_history_list_by_user(user_name_val character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_history_list_by_user(user_name_val character varying) IS 'Получение sql-запроса о изменении объекта в виде :пользователь дата и время изменения:';


--
-- TOC entry 1073 (class 1255 OID 37047)
-- Name: get_sql_history_list_by_user(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_history_list_by_user(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
history_info sys_scheme.table_history_info;
table_scheme_val character varying;
sql_string character varying;
from_string character varying;
where_string character varying;
pk_field_val character varying;
count_history_table integer;
i integer;
BEGIN
	SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;

	sql_string:='SELECT b.id_history, b.dataupd, b.type_operation, b.user_name, b.id, b.id_history_table, b.id_table FROM ';
	from_string:='';
	where_string:='';
	count_history_table:=0;
	i:=1;
	SELECT count(*) INTO count_history_table FROM sys_scheme.table_history_info WHERE id_table = id_table_val;
	IF count_history_table>0 THEN
		FOR history_info IN (SELECT * FROM sys_scheme.table_history_info 
					WHERE id_table = id_table_val ORDER BY id_history_table) LOOP
			from_string:= from_string||' SELECT "'||table_scheme_val||'"."'||history_info.history_table_name||'"."id_history", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."dataupd", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."type_operation", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."user_name",
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."'||pk_field_val||'" as id,
							'||history_info.id_history_table||' as id_history_table,
							'||history_info.id_table||' as id_table
							FROM "'||table_scheme_val||'"."'||history_info.history_table_name||'" ';
			if count_history_table>i THEN
				from_string:= from_string||'UNION';
			END IF;
			i:=i+1;
		END LOOP;
	ELSE
		RETURN '';
	END IF;
	where_string:=';';
	sql_string:=sql_string ||'('||from_string||') b '||where_string;
	RETURN sql_string;
END;$$;


--
-- TOC entry 3477 (class 0 OID 0)
-- Dependencies: 1073
-- Name: FUNCTION get_sql_history_list_by_user(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_history_list_by_user(id_table_val integer) IS 'Получение sql-запроса о изменении объекта в виде :пользователь дата и время изменения:';


--
-- TOC entry 1075 (class 1255 OID 37048)
-- Name: get_sql_history_list_by_user(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_sql_history_list_by_user(id_table_val integer, id_obj_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
history_info sys_scheme.table_history_info;
table_scheme_val character varying;
sql_string character varying;
from_string character varying;
where_string character varying;
pk_field_val character varying;
count_history_table integer;
i integer;
BEGIN
	SELECT pk_fileld INTO pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;

	sql_string:='SELECT b.id_history, b.dataupd, b.type_operation, b.user_name, b.id, b.id_history_table, b.id_table FROM ';
	from_string:='';
	where_string:='';
	count_history_table:=0;
	i:=1;
	SELECT count(*) INTO count_history_table FROM sys_scheme.table_history_info WHERE id_table = id_table_val;
	IF count_history_table>0 THEN
		FOR history_info IN (SELECT * FROM sys_scheme.table_history_info 
					WHERE id_table = id_table_val ORDER BY id_history_table) LOOP
			from_string:= from_string||' SELECT "'||table_scheme_val||'"."'||history_info.history_table_name||'"."id_history", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."dataupd", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."type_operation", 
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."user_name",
							"'||table_scheme_val||'"."'||history_info.history_table_name||'"."'||pk_field_val||'" as id,
							'||history_info.id_history_table||' as id_history_table,
							'||history_info.id_table||' as id_table
							FROM "'||table_scheme_val||'"."'||history_info.history_table_name||'" ';
			if count_history_table>i THEN
				from_string:= from_string||'UNION';
			END IF;
			i:=i+1;
		END LOOP;
	ELSE
		RETURN '';
	END IF;
	where_string:='WHERE b.id = '||id_obj_val||';';
	sql_string:=sql_string ||'('||from_string||') b '||where_string;
	RETURN sql_string;
END;$$;


--
-- TOC entry 3478 (class 0 OID 0)
-- Dependencies: 1075
-- Name: FUNCTION get_sql_history_list_by_user(id_table_val integer, id_obj_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_sql_history_list_by_user(id_table_val integer, id_obj_val integer) IS 'Получение sql-запроса о изменении объекта в виде :пользователь дата и время изменения:';


--
-- TOC entry 1076 (class 1255 OID 37049)
-- Name: get_style_list(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_style_list(id_table_val integer) RETURNS SETOF style_info
    LANGUAGE plpgsql
    AS $$DECLARE
r RECORD;
style RECORD;
temp_val sys_scheme.style_info;
is_reference_val boolean;
type_field_val integer;
sql_string character varying;
ref_table_val integer;
ref_field_val integer;
ref_field_end_val integer;
type_field_val_range integer;
BEGIN
	FOR r IN SELECT id, scheme_name, name_db, map_style, geom_field, 
			       style_field, geom_type, "type", default_style, fontname, fontcolor, 
			       fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, 
			       brushbgcolor, brushfgcolor, brushstyle, brushhatch, view_name, range_colors, range_column, 
			       precision_point, type_color, min_color, min_val, max_color, max_val, 
			       use_min_val, null_color, use_null_color, hidden, use_max_val
			  FROM sys_scheme.table_info WHERE id = id_table_val AND "type"=1 LOOP
		
		temp_val.scheme=r.scheme_name;
		temp_val.tabel_name=r.name_db;
		IF r.default_style = TRUE THEN
			IF r.range_colors = TRUE THEN
				
				temp_val.field_name=null;
				temp_val.type_style=3;
				temp_val.type_val = null;
				temp_val.reference_int_val=null;
				temp_val.interval_begin_int_val=null;
				temp_val.interval_end_int_val=null;
				temp_val.interval_begin_numeric_val=null;
				temp_val.interval_end_numeric_val=null;
				temp_val.type_geom=r.geom_type;
				temp_val.fontname=r.fontname;
				temp_val.fontcolor=r.fontcolor;
				temp_val.fontframecolor=r.fontframecolor;
				temp_val.fontsize=r.fontsize;
				temp_val.symbol=r.symbol;
				temp_val.pencolor=r.pencolor;
				temp_val.pentype=r.pentype;
				temp_val.penwidth=r.penwidth;
				temp_val.brushbgcolor=r.brushbgcolor;
				temp_val.brushfgcolor=r.brushfgcolor;
				temp_val.brushstyle=r.brushstyle;
				temp_val.brushhatch=r.brushhatch;
				    temp_val.range_colors =r.range_colors;
				    temp_val.range_column =r.range_column;
				    SELECT type_field INTO type_field_val_range FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND name_db like r.range_column;

				    if type_field_val_range=1 THEN
					temp_val.precision_point=0;
				    ELSE
					temp_val.precision_point=r.precision_point;
				    END IF;

				    temp_val.type_color =r.type_color;
				    temp_val.min_color =r.min_color;
				    temp_val.min_val =r.min_val;
				    temp_val.max_color =r.max_color;
				    temp_val.max_val =r.max_val;
				    temp_val.use_min_val =r.use_min_val;
				    temp_val.null_color =r.null_color;
				    temp_val.use_null_color =r.use_null_color;
				    temp_val.use_max_val =r.use_max_val;
				RETURN NEXT temp_val;
			ELSE
				temp_val.field_name=null;
				temp_val.type_style=0;
				temp_val.type_val = null;
				temp_val.reference_int_val=null;
				temp_val.interval_begin_int_val=null;
				temp_val.interval_end_int_val=null;
				temp_val.interval_begin_numeric_val=null;
				temp_val.interval_end_numeric_val=null;
				temp_val.type_geom=r.geom_type;
				temp_val.fontname=r.fontname;
				temp_val.fontcolor=r.fontcolor;
				temp_val.fontframecolor=r.fontframecolor;
				temp_val.fontsize=r.fontsize;
				temp_val.symbol=r.symbol;
				temp_val.pencolor=r.pencolor;
				temp_val.pentype=r.pentype;
				temp_val.penwidth=r.penwidth;
				temp_val.brushbgcolor=r.brushbgcolor;
				temp_val.brushfgcolor=r.brushfgcolor;
				temp_val.brushstyle=r.brushstyle;
				temp_val.brushhatch=r.brushhatch;
				RETURN NEXT temp_val;
			END IF;
		ELSE
			temp_val.field_name=r.style_field;
			SELECT is_reference INTO is_reference_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
			IF is_reference_val = TRUE THEN
				temp_val.type_style=1;
				temp_val.type_val=1;
				temp_val.interval_begin_int_val=null;
				temp_val.interval_end_int_val=null;
				temp_val.interval_begin_numeric_val=null;
				temp_val.interval_end_numeric_val=null;
				temp_val.type_geom=r.geom_type;
				SELECT ref_table INTO ref_table_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
				SELECT ref_field INTO ref_field_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
				sql_string:='SELECT "'||sys_scheme.get_table_field_name(ref_field_val)||'" as ref_field, fontname, fontcolor, 
				       fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, 
				       brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM "'||sys_scheme.get_table_scheme_name(ref_table_val)||'"."'||
				       sys_scheme.get_table_name(ref_table_val)||'"';
				raise notice 'SQL1:: %', sql_string;
				FOR style IN EXECUTE sql_string LOOP
					temp_val.reference_int_val=style.ref_field;
					temp_val.fontname=style.fontname;
					temp_val.fontcolor=style.fontcolor;
					temp_val.fontframecolor=style.fontframecolor;
					temp_val.fontsize=style.fontsize;
					temp_val.symbol=style.symbol;
					temp_val.pencolor=style.pencolor;
					temp_val.pentype=style.pentype;
					temp_val.penwidth=style.penwidth;
					temp_val.brushbgcolor=style.brushbgcolor;
					temp_val.brushfgcolor=style.brushfgcolor;
					temp_val.brushstyle=style.brushstyle;
					temp_val.brushhatch=style.brushhatch;
					RETURN NEXT temp_val;
				END LOOP;
			ELSE
				temp_val.type_style=2;
				temp_val.type_geom=r.geom_type;
				SELECT type_field INTO type_field_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND name_db like r.style_field;
				IF type_field_val = 1 THEN
					temp_val.type_val=1;
					SELECT ref_table INTO ref_table_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					SELECT ref_field INTO ref_field_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					SELECT ref_field_end INTO ref_field_end_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					sql_string:='SELECT "'||sys_scheme.get_table_field_name(ref_field_val)||'" as ref_field, 
						"'||sys_scheme.get_table_field_name(ref_field_end_val)||'" as ref_field_end, 
						fontname, fontcolor, 
					       fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, 
					       brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM "'||sys_scheme.get_table_scheme_name(ref_table_val)||'"."'||
						sys_scheme.get_table_name(ref_table_val)||'"';
					raise notice 'SQL2:: %', sql_string;
					FOR style IN EXECUTE sql_string LOOP
						temp_val.reference_int_val=null;
						temp_val.interval_begin_int_val=style.ref_field;
						temp_val.interval_end_int_val=style.ref_field_end;
						temp_val.interval_begin_numeric_val=null;
						temp_val.interval_end_numeric_val=null;
						temp_val.fontname=style.fontname;
						temp_val.fontcolor=style.fontcolor;
						temp_val.fontframecolor=style.fontframecolor;
						temp_val.fontsize=style.fontsize;
						temp_val.symbol=style.symbol;
						temp_val.pencolor=style.pencolor;
						temp_val.pentype=style.pentype;
						temp_val.penwidth=style.penwidth;
						temp_val.brushbgcolor=style.brushbgcolor;
						temp_val.brushfgcolor=style.brushfgcolor;
						temp_val.brushstyle=style.brushstyle;
						temp_val.brushhatch=style.brushhatch;
						RETURN NEXT temp_val;
					END LOOP;
				ELSE
					temp_val.type_val=2;
					temp_val.type_geom=r.geom_type;
					SELECT ref_table INTO ref_table_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					SELECT ref_field INTO ref_field_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					SELECT ref_field_end INTO ref_field_end_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					sql_string:='SELECT "'||sys_scheme.get_table_field_name(ref_field_val)||'" as ref_field, 
						"'||sys_scheme.get_table_field_name(ref_field_end_val)||'" as ref_field_end, 
						fontname, fontcolor, 
					       fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, 
					       brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM "'||sys_scheme.get_table_scheme_name(ref_table_val)||'"."'||
							sys_scheme.get_table_name(ref_table_val)||'"';
					raise notice 'SQL3:: %', sql_string;
					FOR style IN EXECUTE sql_string LOOP
						temp_val.reference_int_val=null;
						temp_val.interval_begin_int_val=null;
						temp_val.interval_end_int_val=null;
						temp_val.interval_begin_numeric_val=style.ref_field;
						temp_val.interval_end_numeric_val=style.ref_field;
						temp_val.fontname=style.fontname;
						temp_val.fontcolor=style.fontcolor;
						temp_val.fontframecolor=style.fontframecolor;
						temp_val.fontsize=style.fontsize;
						temp_val.symbol=style.symbol;
						temp_val.pencolor=style.pencolor;
						temp_val.pentype=style.pentype;
						temp_val.penwidth=style.penwidth;
						temp_val.brushbgcolor=style.brushbgcolor;
						temp_val.brushfgcolor=style.brushfgcolor;
						temp_val.brushstyle=style.brushstyle;
						temp_val.brushhatch=style.brushhatch;
						RETURN NEXT temp_val;
					END LOOP;
				END IF;
				
			END IF;
		END IF;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3479 (class 0 OID 0)
-- Dependencies: 1076
-- Name: FUNCTION get_style_list(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_style_list(id_table_val integer) IS 'Получение списка стилей к таблице';


--
-- TOC entry 1077 (class 1255 OID 37050)
-- Name: get_table_field_name(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_field_name(id_field_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE

table_field_val character varying;

BEGIN

SELECT name_db INTO table_field_val FROM sys_scheme.table_field_info WHERE id = id_field_val;

RETURN table_field_val;
END;$$;


--
-- TOC entry 3480 (class 0 OID 0)
-- Dependencies: 1077
-- Name: FUNCTION get_table_field_name(id_field_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_field_name(id_field_val integer) IS 'Получение название схемы таблицы в базе';


--
-- TOC entry 171 (class 1259 OID 37051)
-- Name: table_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_info (
    id integer NOT NULL,
    scheme_name character varying,
    name_db character varying NOT NULL,
    name_map character varying NOT NULL,
    lablefiled character varying,
    map_style boolean DEFAULT false NOT NULL,
    geom_field character varying DEFAULT 'geom'::character varying NOT NULL,
    style_field character varying DEFAULT 'style'::character varying NOT NULL,
    geom_type integer NOT NULL,
    type integer NOT NULL,
    default_style boolean DEFAULT true NOT NULL,
    fontname character varying DEFAULT 'Map Symbols'::character varying,
    fontcolor integer DEFAULT 16711680,
    fontframecolor integer DEFAULT 16711680,
    fontsize integer DEFAULT 12,
    symbol integer DEFAULT 35,
    pencolor integer DEFAULT 16711680,
    pentype integer DEFAULT 2,
    penwidth integer DEFAULT 1,
    brushbgcolor bigint DEFAULT 16711680,
    brushfgcolor integer DEFAULT 16711680,
    brushstyle integer DEFAULT 0,
    brushhatch integer DEFAULT 1,
    read_only boolean DEFAULT false NOT NULL,
    photo boolean DEFAULT false,
    id_style integer DEFAULT 0,
    pk_fileld character varying DEFAULT 'id'::character varying NOT NULL,
    is_style boolean DEFAULT false,
    source_layer boolean DEFAULT false,
    image_column character varying DEFAULT ''::character varying,
    angle_column character varying DEFAULT ''::character varying,
    use_bounds boolean DEFAULT false,
    min_scale integer DEFAULT 0,
    max_scale integer DEFAULT 0,
    id_group integer DEFAULT 0 NOT NULL,
    default_visibl boolean DEFAULT false,
    sql_view_string character varying,
    order_num integer DEFAULT 0,
    view_name character varying,
    masterdb_history_id integer,
    connection_string text,
    remote_lgn text,
    remote_pwd text,
    fixed_history_id integer,
    range_colors boolean DEFAULT false,
    range_column character varying,
    precision_point integer DEFAULT 1,
    type_color integer DEFAULT 0,
    min_color bigint DEFAULT 0,
    min_val integer,
    max_color bigint DEFAULT 0,
    max_val integer,
    use_min_val boolean DEFAULT false,
    null_color bigint DEFAULT 0,
    use_null_color boolean DEFAULT false,
    hidden boolean DEFAULT false NOT NULL,
    use_max_val boolean DEFAULT false,
    label_showframe boolean DEFAULT false NOT NULL,
    label_framecolor integer DEFAULT 0 NOT NULL,
    label_parallel boolean DEFAULT true NOT NULL,
    label_overlap boolean DEFAULT true NOT NULL,
    label_usebounds boolean DEFAULT false NOT NULL,
    label_minscale integer DEFAULT 0 NOT NULL,
    label_maxscale integer DEFAULT 0 NOT NULL,
    label_offset integer DEFAULT 0 NOT NULL,
    label_graphicunits boolean DEFAULT false NOT NULL,
    label_fontname character varying DEFAULT 'Times New Roman'::character varying NOT NULL,
    label_fontcolor integer DEFAULT 0 NOT NULL,
    label_fontsize integer DEFAULT 9 NOT NULL,
    label_fontstrikeout boolean DEFAULT false NOT NULL,
    label_fontitalic boolean DEFAULT false NOT NULL,
    label_fontunderline boolean DEFAULT false NOT NULL,
    label_fontbold boolean DEFAULT false NOT NULL,
    label_uselabelstyle boolean DEFAULT false NOT NULL,
    label_showlabel boolean DEFAULT false NOT NULL,
    min_object_size integer DEFAULT 0 NOT NULL,
    ref_table integer,
    graphic_units boolean DEFAULT false,
    display_when_opening boolean DEFAULT true NOT NULL
);


--
-- TOC entry 3481 (class 0 OID 0)
-- Dependencies: 171
-- Name: TABLE table_info; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE table_info IS 'Основная информация по таблицам';


--
-- TOC entry 172 (class 1259 OID 37117)
-- Name: table_type_table; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_type_table (
    id integer NOT NULL,
    name character varying NOT NULL,
    map_layer boolean DEFAULT false NOT NULL
);


--
-- TOC entry 3482 (class 0 OID 0)
-- Dependencies: 172
-- Name: TABLE table_type_table; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE table_type_table IS 'Тип таблицы';


--
-- TOC entry 173 (class 1259 OID 37124)
-- Name: table_vw; Type: VIEW; Schema: sys_scheme; Owner: -
--

CREATE VIEW table_vw AS
    SELECT table_info.id, table_info.scheme_name, table_info.name_db, table_info.name_map, table_info.geom_field, table_info.geom_type, table_info.type AS table_type, table_type_table.map_layer AS is_layer, table_type_table.name AS type_name, table_info.read_only, table_info.photo, table_info.pk_fileld FROM table_info, table_type_table WHERE (table_type_table.id = table_info.type);


--
-- TOC entry 1078 (class 1255 OID 37129)
-- Name: get_table_list(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_list() RETURNS SETOF table_vw
    LANGUAGE plpgsql
    AS $$DECLARE

r RECORD;

BEGIN

	FOR r IN SELECT id, scheme_name, name_db, name_map, geom_field, geom_type, table_type, 
				is_layer, type_name, read_only, photo, pk_fileld, view_name
			FROM sys_scheme.table_vw LOOP
		RETURN NEXT r;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3483 (class 0 OID 0)
-- Dependencies: 1078
-- Name: FUNCTION get_table_list(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_list() IS 'Получение списка таблиц';


--
-- TOC entry 1079 (class 1255 OID 37130)
-- Name: get_table_name(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_name(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE

table_name_val character varying;

BEGIN

SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;

RETURN table_name_val;
END;$$;


--
-- TOC entry 3484 (class 0 OID 0)
-- Dependencies: 1079
-- Name: FUNCTION get_table_name(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_name(id_table_val integer) IS 'Получение название таблицы в базе';


--
-- TOC entry 1080 (class 1255 OID 37131)
-- Name: get_table_pkfield(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_pkfield(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE
table_pkfield character varying;
BEGIN

	SELECT pk_fileld INTO table_pkfield FROM sys_scheme.table_info WHERE id = id_table_val;

RETURN table_pkfield;
END;$$;


--
-- TOC entry 3485 (class 0 OID 0)
-- Dependencies: 1080
-- Name: FUNCTION get_table_pkfield(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_pkfield(id_table_val integer) IS 'Получение первичного ключа';


--
-- TOC entry 1081 (class 1255 OID 37132)
-- Name: get_table_scheme_name(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_scheme_name(id_table_val integer) RETURNS character varying
    LANGUAGE plpgsql
    AS $$DECLARE

table_scheme_val character varying;

BEGIN

SELECT scheme_name INTO table_scheme_val FROM sys_scheme.table_info WHERE id = id_table_val;

RETURN table_scheme_val;
END;$$;


--
-- TOC entry 3486 (class 0 OID 0)
-- Dependencies: 1081
-- Name: FUNCTION get_table_scheme_name(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_scheme_name(id_table_val integer) IS 'Получение название схемы таблицы в базе';


--
-- TOC entry 1082 (class 1255 OID 37133)
-- Name: get_table_seq_name(character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_table_seq_name(scheme_name character varying, table_name character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$


DECLARE
table_name_val character varying;
BEGIN
SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
RETURN table_name_val;
END;


$$;


--
-- TOC entry 3487 (class 0 OID 0)
-- Dependencies: 1082
-- Name: FUNCTION get_table_seq_name(scheme_name character varying, table_name character varying); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_table_seq_name(scheme_name character varying, table_name character varying) IS 'Получение название sequence для таблицы scheme_name.table_name';


--
-- TOC entry 174 (class 1259 OID 37134)
-- Name: user_db; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE user_db (
    id integer NOT NULL,
    name character varying NOT NULL,
    pass character varying NOT NULL,
    name_full character varying NOT NULL,
    login character varying NOT NULL,
    otdel character varying,
    admin boolean DEFAULT false NOT NULL,
    id_rajon integer DEFAULT 0,
    glava boolean DEFAULT false NOT NULL,
    window_name character varying DEFAULT 'РЕКОД Инфраструктура'::character varying,
    typ integer DEFAULT 2
);


--
-- TOC entry 175 (class 1259 OID 37145)
-- Name: user_vw; Type: VIEW; Schema: sys_scheme; Owner: -
--

CREATE VIEW user_vw AS
    SELECT user_db.id, user_db.name_full, user_db.login AS login_user, user_db.admin AS is_admin, user_db.typ AS user_type FROM user_db;


--
-- TOC entry 1083 (class 1255 OID 37149)
-- Name: get_user_list(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_user_list() RETURNS SETOF user_vw
    LANGUAGE plpgsql
    AS $$DECLARE

r RECORD;

BEGIN

	FOR r IN SELECT id, name_full, login_user, is_admin, user_type FROM sys_scheme.user_vw loop
		RETURN NEXT r;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3488 (class 0 OID 0)
-- Dependencies: 1083
-- Name: FUNCTION get_user_list(); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_user_list() IS 'Получение списка пользователей';


--
-- TOC entry 176 (class 1259 OID 37150)
-- Name: table_right; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_right (
    id integer NOT NULL,
    id_table integer NOT NULL,
    id_user integer NOT NULL,
    read_data boolean DEFAULT false NOT NULL,
    write_data boolean DEFAULT false NOT NULL
);


--
-- TOC entry 177 (class 1259 OID 37155)
-- Name: user_right_wv; Type: VIEW; Schema: sys_scheme; Owner: -
--

CREATE VIEW user_right_wv AS
    SELECT table_right.id, table_right.id_table, table_info.scheme_name, table_info.name_db, table_right.id_user, user_db.login AS login_user, table_right.read_data, table_right.write_data FROM table_right, user_db, table_info WHERE ((user_db.id = table_right.id_user) AND (table_info.id = table_right.id_table));


--
-- TOC entry 1084 (class 1255 OID 37160)
-- Name: get_user_right(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION get_user_right(user_id integer, table_id integer) RETURNS SETOF user_right_wv
    LANGUAGE plpgsql
    AS $$DECLARE

r RECORD;

BEGIN
	FOR r IN SELECT id, id_table, scheme_name, name_db, id_user, login_user, read_data, write_data FROM sys_scheme.user_right_wv WHERE id_table = table_id AND id_user= user_id LOOP
		RETURN NEXT r;
	END LOOP;
	RETURN;
END;$$;


--
-- TOC entry 3489 (class 0 OID 0)
-- Dependencies: 1084
-- Name: FUNCTION get_user_right(user_id integer, table_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION get_user_right(user_id integer, table_id integer) IS 'Получение прав на таблицу';


--
-- TOC entry 1085 (class 1255 OID 37161)
-- Name: grand_all_public_to_lo(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION grand_all_public_to_lo() RETURNS character varying
    LANGUAGE plpgsql
    AS $$
DECLARE
rec record;
table_name_val character varying;
BEGIN
FOR rec in SELECT fileid FROM sys_scheme.upd_files LOOP
	EXECUTE 'GRANT ALL ON LARGE OBJECT '||rec.fileid||' TO public';
END LOOP;
FOR rec in SELECT fileid FROM sys_scheme.upd_user_files LOOP
	EXECUTE 'GRANT ALL ON LARGE OBJECT '||rec.fileid||' TO public';
END LOOP;
RETURN 'ok';
END;
$$;


--
-- TOC entry 1086 (class 1255 OID 37162)
-- Name: history_photo_insrt(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION history_photo_insrt() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
id_table_photo_val integer;
BEGIN
SELECT ti.id INTO id_table_photo_val FROM sys_scheme.table_photo_info tpi, sys_scheme.table_info ti WHERE ti.scheme_name like TG_TABLE_SCHEMA 
					AND ti.id= tpi.id_table AND tpi.photo_table like TG_TABLE_NAME;
	IF TG_OP='INSERT' THEN
		INSERT INTO sys_scheme.table_history_photo(
		    id_table_photo, id_photo, id_obj, type_operation, user_name, 
		    dataupdate, file, img_preview, file_name, is_photo, master_id)
		VALUES (id_table_photo_val, NEW.id, NEW.id_obj, 1, current_user, 
			now(), null, NEW.img_preview, NEW.file_name, NEW.is_photo, NEW.master_id);
		return NEW;
	END IF;
	IF TG_OP='UPDATE' THEN
			INSERT INTO sys_scheme.table_history_photo(
		    id_table_photo, id_photo, id_obj, type_operation, user_name, 
		    dataupdate, file, img_preview, file_name, is_photo, master_id)
		VALUES (id_table_photo_val, NEW.id, NEW.id_obj, 2, current_user, 
			now(), null, NEW.img_preview, NEW.file_name, NEW.is_photo, NEW.master_id);
		return NEW;
	END IF;
	IF TG_OP='DELETE' THEN
		INSERT INTO sys_scheme.table_history_photo(
		    id_table_photo, id_photo, id_obj, type_operation, user_name, 
		    dataupdate, file, img_preview, file_name, is_photo, master_id)
		VALUES (id_table_photo_val, OLD.id, OLD.id_obj, 3, current_user, 
			now(), OLD.file, OLD.img_preview, OLD.file_name, OLD.is_photo, OLD.master_id);
		return OLD;
	END IF;
END;$$;


--
-- TOC entry 1047 (class 1255 OID 37163)
-- Name: insert_after_field(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION insert_after_field() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
cnt integer;
BEGIN
	PERFORM sys_scheme.create_view_for_table(NEW.id_table);
	RETURN NEW;
END;$$;


--
-- TOC entry 1068 (class 1255 OID 37164)
-- Name: insrt_new_field(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION insrt_new_field() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
cnt integer;
BEGIN
	SELECT max(num_order)+1 INTO cnt FROM sys_scheme.table_field_info WHERE id_table = new.id_table AND num_order is not NULL;
	if cnt is not NULL THEN
		new.num_order = cnt;
	ELSE
		new.num_order = 0;
	END IF;
	
	return NEW;
END;$$;


--
-- TOC entry 1087 (class 1255 OID 37165)
-- Name: insrt_new_table(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION insrt_new_table() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
cnt integer;
BEGIN
	--INSERT INTO sys_scheme.table_right(id_table, id_user, read_data, write_data)
	--SELECT NEW.id, u.id, true, true FROM sys_scheme.user_db u WHERE (u.admin=true OR u.typ=1);

	SELECT count(*) INTO cnt FROM sys_scheme.table_schems WHERE "name" = NEW.scheme_name;
	IF cnt=0 THEN
		INSERT INTO sys_scheme.table_schems ("name") VALUES (NEW.scheme_name);
	END IF;
	IF NEW."type"=1 THEN
		UPDATE sys_scheme.table_info SET source_layer = true WHERE id = NEW.id;
	END IF;

	SELECT max(order_num)+1 INTO cnt FROM sys_scheme.table_info WHERE default_visibl = true AND order_num is not NULL;
	if cnt is not NULL THEN
		new.order_num = cnt;
	ELSE
		new.order_num = 0;
	END IF;
	--UPDATE sys_scheme.table_info SET view_name = sys_scheme.create_view_for_table(NEW.id) WHERE id = NEW.id;
	RETURN NEW;
	
END;$$;


--
-- TOC entry 1088 (class 1255 OID 37166)
-- Name: insrt_new_table_in_group(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION insrt_new_table_in_group() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
cnt integer;
BEGIN
	SELECT max(order_num)+1 INTO cnt FROM sys_scheme.table_groups_table WHERE id_group = new.id_group;
	if cnt is not NULL THEN
		new.order_num = cnt;
	ELSE
		new.order_num = 0;
	END IF;
	return NEW;
END;$$;


--
-- TOC entry 1089 (class 1255 OID 37167)
-- Name: insrt_user(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION insrt_user() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
	login_user character varying;
	pass_user character varying;
	rec sys_scheme.table_schems;
	exists_group boolean;
BEGIN
	login_user = NEW."login";
	pass_user = NEW.pass;
	IF NEW.typ=1 THEN
		NEW."admin" = TRUE;
		SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_admin_group()) INTO exists_group;
		IF exists_group=FALSE THEN
			PERFORM sys_scheme.create_admin_group();
		END IF;
		EXECUTE 'CREATE ROLE ' || login_user || ' LOGIN ENCRYPTED PASSWORD ''' || pass_user || ''' NOSUPERUSER INHERIT CREATEDB CREATEROLE;';
		EXECUTE 'GRANT '|| sys_scheme.get_admin_group() ||' TO ' || login_user;
	END IF;
	IF NEW.typ=2 THEN
		NEW."admin" = FALSE;
		EXECUTE 'CREATE ROLE ' || login_user || ' LOGIN ENCRYPTED PASSWORD ''' || pass_user || ''' NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE';
	END IF;
	--
	SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_client_group()) INTO exists_group;
	IF exists_group=FALSE THEN
		PERFORM sys_scheme.create_client_group();
	END IF;
	EXECUTE 'GRANT '|| sys_scheme.get_client_group() ||' TO ' ||  login_user;
	--EXECUTE 'GRANT CONNECT ON DATABASE ' || current_database() || ' TO ' || login_user;		
	/*
	FOR rec IN (SELECT * FROM sys_scheme.table_schems) LOOP
		EXECUTE 'GRANT ALL ON SCHEMA "'||rec."name"||'" TO ' || login_user;
	END LOOP;
	*/
	RETURN NEW;
END;$$;


--
-- TOC entry 1090 (class 1255 OID 37168)
-- Name: rename_field(integer, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION rename_field(id_field integer, new_name text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
IF new_name is not null AND new_name<>'' THEN
	UPDATE sys_scheme.table_field_info SET name_map = new_name WHERE id = id_field;
END IF;
RETURN true;
END;$$;


--
-- TOC entry 1091 (class 1255 OID 37169)
-- Name: rename_field(text, text, text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION rename_field(scheme_name_val text, table_name_val text, field_name_val text, new_name text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
IF new_name is not null AND new_name<>'' THEN
	UPDATE sys_scheme.table_field_info SET name_map = new_name FROM sys_scheme.table_info ti 
		WHERE ti.scheme_name = scheme_name_val AND ti.name_db = table_name_val 
		AND sys_scheme.table_field_info.id_table = ti.id 
		AND sys_scheme.table_field_info.name_db = field_name_val;
END IF;
RETURN true;
END;$$;


--
-- TOC entry 1092 (class 1255 OID 37170)
-- Name: rename_table(integer, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION rename_table(id_table integer, new_name text) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
IF new_name is not null AND new_name<>'' THEN
	UPDATE sys_scheme.table_info SET name_map = new_name WHERE id = id_table;
END IF;
RETURN true;
END;$$;


--
-- TOC entry 1093 (class 1255 OID 37171)
-- Name: set_photo_history(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_photo_history(table_id integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
table_shemename character varying;
table_namedb character varying;
exists_photo_table boolean;
exists_photo_trigger boolean; 
BEGIN
	exists_photo_trigger:=FALSE;
	SELECT scheme_name INTO table_shemename FROM sys_scheme.table_info WHERE id = table_id;
	SELECT photo INTO exists_photo_table FROM sys_scheme.table_info WHERE id = table_id;
	SELECT photo_table INTO table_namedb FROM sys_scheme.table_photo_info WHERE id_table = table_id;
	SELECT EXISTS(SELECT true FROM information_schema.triggers WHERE trigger_schema like table_shemename AND trigger_name LIKE 'insert_history_'||table_namedb) INTO exists_photo_trigger;

	IF exists_photo_table = TRUE AND exists_photo_trigger=FALSE THEN
		raise notice 'Даааа %', 'CREATE TRIGGER insert_history_'||table_namedb||' '||
		  'BEFORE INSERT OR UPDATE OR DELETE '||
		  'ON '||table_shemename||'.'||table_namedb||' '||
		  'FOR EACH ROW '||
		  'EXECUTE PROCEDURE sys_scheme.history_photo_insrt();';
		EXECUTE 'CREATE TRIGGER insert_history_'||table_namedb||' '||
		  'BEFORE INSERT OR UPDATE OR DELETE '||
		  'ON '||table_shemename||'.'||table_namedb||' '||
		  'FOR EACH ROW '||
		  'EXECUTE PROCEDURE sys_scheme.history_photo_insrt();';
		RETURN TRUE;
	ELSE
		RETURN FALSE;
	END IF;
	RETURN FALSE;
END;$$;


--
-- TOC entry 3490 (class 0 OID 0)
-- Dependencies: 1093
-- Name: FUNCTION set_photo_history(table_id integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION set_photo_history(table_id integer) IS 'Установка историии для таблицы с файлами';


--
-- TOC entry 1127 (class 1255 OID 68583)
-- Name: set_photo_history(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_photo_history(table_id integer, iswork boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
table_shemename character varying;
table_namedb character varying;
exists_photo_table boolean;
exists_photo_trigger boolean; 
BEGIN
	exists_photo_trigger:=FALSE;
	SELECT scheme_name INTO table_shemename FROM sys_scheme.table_info WHERE id = table_id;
	SELECT photo INTO exists_photo_table FROM sys_scheme.table_info WHERE id = table_id;
	SELECT photo_table INTO table_namedb FROM sys_scheme.table_photo_info WHERE id_table = table_id;
	SELECT EXISTS(SELECT true FROM information_schema.triggers WHERE trigger_schema like table_shemename AND trigger_name LIKE 'insert_history_'||table_namedb) INTO exists_photo_trigger;

	IF iswork=TRUE THEN
		IF exists_photo_table = TRUE AND exists_photo_trigger=FALSE THEN
			raise notice 'Даааа %', 'CREATE TRIGGER insert_history_'||table_namedb||' '||
			  'BEFORE INSERT OR UPDATE OR DELETE '||
			  'ON '||table_shemename||'.'||table_namedb||' '||
			  'FOR EACH ROW '||
			  'EXECUTE PROCEDURE sys_scheme.history_photo_insrt();';
			EXECUTE 'CREATE TRIGGER insert_history_'||table_namedb||' '||
			  'BEFORE INSERT OR UPDATE OR DELETE '||
			  'ON '||table_shemename||'.'||table_namedb||' '||
			  'FOR EACH ROW '||
			  'EXECUTE PROCEDURE sys_scheme.history_photo_insrt();';
			RETURN TRUE;
		ELSE
			RETURN FALSE;
		END IF;
	ELSE
		IF exists_photo_table = TRUE AND exists_photo_trigger=TRUE THEN
			raise notice 'Даааа %', 'DROP TRIGGER insert_history_'||table_namedb||' ON '||table_shemename||'.'||table_namedb||';';
			EXECUTE 'DROP TRIGGER insert_history_'||table_namedb||' ON '||table_shemename||'.'||table_namedb||';';
			RETURN TRUE;
		ELSE
			RETURN FALSE;
		END IF;
	END IF;
	RETURN FALSE;
END;$$;


--
-- TOC entry 3491 (class 0 OID 0)
-- Dependencies: 1127
-- Name: FUNCTION set_photo_history(table_id integer, iswork boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION set_photo_history(table_id integer, iswork boolean) IS 'Установка/удаление историии для таблицы с файлами';


--
-- TOC entry 1094 (class 1255 OID 37172)
-- Name: set_photo_table(integer, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_photo_table(table_id integer, exists_photo boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_photo_table boolean;
exists_photo_meta boolean;
photo_table_name text;
scheme_db_name text;
table_db_name text;
table_pk_field text;
BEGIN
	SELECT scheme_name, name_db, pk_fileld INTO scheme_db_name, table_db_name, table_pk_field FROM sys_scheme.table_info WHERE id=table_id;
	IF exists_photo=FALSE THEN
		UPDATE sys_scheme.table_info SET photo = false WHERE id = table_id;
	ELSE
		SELECT exists(SELECT true FROM sys_scheme.table_photo_info WHERE id_table = table_id) INTO exists_photo_meta;
		IF exists_photo_meta=true THEN
			SELECT photo_table INTO photo_table_name FROM sys_scheme.table_photo_info WHERE id_table = table_id;
			PERFORM sys_scheme.create_photo_table(scheme_db_name, photo_table_name, table_db_name, table_pk_field);
			UPDATE sys_scheme.table_info SET photo = true WHERE id = table_id;
		ELSE
			PERFORM sys_scheme.create_photo_table(scheme_db_name, 'photo_'||table_db_name, table_db_name, table_pk_field);
			INSERT INTO sys_scheme.table_photo_info(id_table, photo_table, photo_field, photo_file, id_field_tble)
				VALUES (table_id, 'photo_'||table_db_name, 'id_obj', 'file', 'id');
			UPDATE sys_scheme.table_info SET photo = true WHERE id = table_id;
		END IF;
	END IF;
	RETURN true;
END;$$;


--
-- TOC entry 3492 (class 0 OID 0)
-- Dependencies: 1094
-- Name: FUNCTION set_photo_table(table_id integer, exists_photo boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION set_photo_table(table_id integer, exists_photo boolean) IS 'Добавление и удаление возможности прикрепления файлов';


--
-- TOC entry 1095 (class 1255 OID 37173)
-- Name: set_right(integer, integer, boolean, boolean); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_right(table_id integer, user_id integer, read_val boolean, write_val boolean) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_item boolean;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_right WHERE id_table = table_id AND id_user = user_id) INTO exists_item;
	IF exists_item = true THEN
		UPDATE sys_scheme.table_right SET read_data = read_val, write_data = write_val WHERE id_table = table_id AND id_user = user_id;
	ELSE
		INSERT INTO sys_scheme.table_right (id_table, id_user, read_data, write_data) VALUES (table_id, user_id, read_val, write_val);
	END IF;

RETURN true;
END;$$;


--
-- TOC entry 3493 (class 0 OID 0)
-- Dependencies: 1095
-- Name: FUNCTION set_right(table_id integer, user_id integer, read_val boolean, write_val boolean); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION set_right(table_id integer, user_id integer, read_val boolean, write_val boolean) IS 'Назначение прав на таблицу Инфраструктуры';


--
-- TOC entry 1096 (class 1255 OID 37174)
-- Name: set_right_to_table(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_right_to_table() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
table_name character varying;
photo_table_name character varying;
scheme_name_trg character varying;
pk_field character varying;
user_name character varying;
exists_photo boolean;
exists_seq boolean;
BEGIN
	SELECT ti.name_db, ti.pk_fileld, ti.scheme_name, ti.photo 
	into table_name, pk_field, scheme_name_trg, exists_photo
	FROM sys_scheme.table_info ti WHERE ti.id = NEW.id_table;
select photo_table into photo_table_name from sys_scheme.table_photo_info where id_table=NEW.id_table;
	SELECT ud."login" INTO user_name FROM sys_scheme.user_db ud WHERE ud.id = NEW.id_user;
	SELECT EXISTS(SELECT true FROM information_schema.sequences WHERE sequence_schema like scheme_name_trg
								AND sequence_name like table_name||'_'||pk_field||'_seq') INTO exists_seq;
	IF NEW.read_data=true AND NEW.write_data=false THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'" FROM ' || user_name;
		EXECUTE 'GRANT SELECT ON '||scheme_name_trg||'."'||table_name||'" TO ' || user_name;
		IF exists_photo=true THEN
			EXECUTE 'GRANT SELECT ON '||scheme_name_trg||'."'||photo_table_name||'" TO ' || user_name;
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" TO ' || user_name;
		END IF;
	END IF;
	IF (NEW.read_data=true AND NEW.write_data=true) or (NEW.read_data=false AND NEW.write_data=true) THEN
		EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||table_name||'" TO ' || user_name;
		IF exists_seq THEN
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||table_name||'_'||pk_field||'_seq" TO ' || user_name;
		END IF;
		IF exists_photo=true THEN
			EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||photo_table_name||'" TO ' || user_name;
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" TO ' || user_name;
		END IF;
	END IF;
	IF (NEW.read_data=false AND NEW.write_data=false) THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'" FROM ' || user_name;
		IF exists_seq THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'_'||pk_field||'_seq" FROM ' || user_name;
		END IF;
		IF exists_photo=true THEN
			EXECUTE 'REVOKE SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||photo_table_name||'" FROM ' || user_name;
			EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" FROM ' || user_name;
		END IF;
	END IF;
RETURN NEW;
END;$$;


--
-- TOC entry 1097 (class 1255 OID 37175)
-- Name: set_table_rights_for_admins(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_table_rights_for_admins(id_table_val integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_table_val boolean;
rec RECORD;
BEGIN
	PERFORM sys_scheme.set_right(id_table_val, u.id, true, true) FROM sys_scheme.user_db u WHERE u.typ=1;
RETURN true;
END;$$;


--
-- TOC entry 1098 (class 1255 OID 37176)
-- Name: set_value_photo(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION set_value_photo() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	IF TG_OP='INSERT' THEN
		UPDATE sys_scheme.table_info SET photo = true WHERE id = NEW.id_table;
		return NEW;
	END IF;
	IF TG_OP='UPDATE' THEN
		UPDATE sys_scheme.table_info SET photo = true WHERE id = NEW.id_table;
		return NEW;
	END IF;
	IF TG_OP='DELETE' THEN
		UPDATE sys_scheme.table_info SET photo = false WHERE id = OLD.id_table;
		return OLD;
	END IF;
END;$$;


--
-- TOC entry 1099 (class 1255 OID 37177)
-- Name: stop_history(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION stop_history(id_table_val integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
table_name_val character varying;
scheme_name_val character varying;
BEGIN
	SELECT name_db INTO table_name_val FROM sys_scheme.table_info WHERE id = id_table_val;
	SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;

	EXECUTE 'ALTER TABLE "'||scheme_name_val||'"."'||table_name_val||'" DISABLED TRIGGER';
	
RETURN true;
END;$$;


--
-- TOC entry 3494 (class 0 OID 0)
-- Dependencies: 1099
-- Name: FUNCTION stop_history(id_table_val integer); Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON FUNCTION stop_history(id_table_val integer) IS 'Остановка ведение истории';


--
-- TOC entry 1100 (class 1255 OID 37178)
-- Name: super_fix_db(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION super_fix_db() RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	PERFORM sys_scheme.fix_groups();
	PERFORM sys_scheme.fix_user_for_groups();
	PERFORM sys_scheme.fix_sys_scheme();
	PERFORM sys_scheme.fix_table_grants();
	PERFORM sys_scheme.fix_table_owner();
	PERFORM sys_scheme.fix_geom_indexes();
	PERFORM sys_scheme.fix_sequence_numbers();
	PERFORM sys_scheme.fix_geom_indexes();
	PERFORM sys_scheme.fix_geomtry_colums();
RETURN true;
END;$$;


--
-- TOC entry 1101 (class 1255 OID 37179)
-- Name: super_fix_table_sync_fields(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION super_fix_table_sync_fields(id_table_val integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
exists_table_val boolean;
rec RECORD;
BEGIN
	FOR rec IN SELECT id, scheme_name, name_db FROM sys_scheme.table_info WHERE id = id_table_val LOOP
		SELECT EXISTS(SELECT true FROM information_schema.tables WHERE table_schema like rec.scheme_name 
						AND table_name like rec.name_db
						AND table_type like 'BASE TABLE') INTO exists_table_val;
		IF exists_table_val=true THEN
			PERFORM sys_scheme.create_sync_field(rec.id);
			PERFORM sys_scheme.create_sync_field_for_photo(rec.id);
		END IF;
	END LOOP;
RETURN true;
END;$$;


--
-- TOC entry 1102 (class 1255 OID 37180)
-- Name: unlink_before_delete(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION unlink_before_delete() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE 
 cnt integer; 
 lnt integer;  
 BEGIN 
     SELECT count(fileid) INTO cnt FROM  
         (SELECT fileid FROM sys_scheme.upd_user_files WHERE fileid=OLD.fileid UNION ALL 
          SELECT fileid FROM sys_scheme.upd_files WHERE fileid=OLD.fileid UNION ALL 
          SELECT fileid FROM sys_scheme.upd_updater WHERE fileid=OLD.fileid) as file_table; 
     IF cnt=1 THEN 
         SELECT count(loid) INTO lnt FROM (SELECT DISTINCT loid FROM pg_largeobject WHERE loid = OLD.fileid) as lo_table; 
         IF lnt=1 THEN 
             perform lo_unlink(OLD.fileid);  
         END IF;  
     END IF; 
     return OLD; 
 END;$$;


--
-- TOC entry 1103 (class 1255 OID 37181)
-- Name: upd_num_order(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION upd_num_order() RETURNS boolean
    LANGUAGE plpgsql
    AS $$
DECLARE
   tmp2 record;
   count integer;
   idt integer;
BEGIN
   count = 1;
   idt = -1;
   FOR tmp2 IN SELECT id, id_table FROM sys_scheme.table_field_info ORDER BY id_table, id LOOP
      IF idt <> tmp2.id_table THEN 
         idt = tmp2.id_table;
         count = 1;
      END IF;
      UPDATE sys_scheme.table_field_info SET num_order = count WHERE id = tmp2.id;
      count = count + 1;
   END LOOP;
   RETURN true;
END;

$$;


--
-- TOC entry 1104 (class 1255 OID 37186)
-- Name: user_change_fio(integer, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION user_change_fio(user_id integer, new_fio character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
BEGIN
	IF new_fio is not null AND new_fio <> '' THEN
		UPDATE sys_scheme.user_db SET name_full =  new_fio WHERE id = user_id;
	END IF;
	return TRUE;
END;$$;


--
-- TOC entry 1105 (class 1255 OID 37187)
-- Name: user_change_login(integer, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION user_change_login(user_id integer, new_login character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
	user_login text;
BEGIN
	SELECT "login" INTO user_login FROM sys_scheme.user_db WHERE id = user_id;
	IF (user_login is NULL) THEN
		RAISE EXCEPTION '|%|����� ������������ �� ������',12;
	END IF;
	
	EXECUTE 'ALTER ROLE ' || user_login || ' RENAME TO ' || new_login || ';';
	
	UPDATE sys_scheme.user_db SET "login" = new_login WHERE id = user_id;
	return TRUE;
END;$$;


--
-- TOC entry 1106 (class 1255 OID 37188)
-- Name: user_change_pw(integer, character varying, character varying); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION user_change_pw(user_id integer, user_pass character varying, user_pass_sync character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS $$DECLARE
	user_login text;
BEGIN
	SELECT "login" INTO user_login FROM sys_scheme.user_db WHERE id = user_id;
	IF (user_login is NULL) THEN
		RAISE EXCEPTION '|%|����� ������������ �� ������',12;
	END IF;
	EXECUTE 'ALTER Role ' || user_login || ' ENCRYPTED PASSWORD ''' || user_pass || ''';';
	
	UPDATE sys_scheme.user_db SET pass = user_pass_sync WHERE id = user_id;
	return TRUE;
END;$$;


--
-- TOC entry 1107 (class 1255 OID 37189)
-- Name: userlocations_add_location(text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_add_location(location_name text, location_extent text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
 ret_id integer;
BEGIN
 insert into sys_scheme.map_extents(name,extent)
  values (location_name,location_extent)
  returning id into ret_id;
RETURN ret_id;
END;
$$;


--
-- TOC entry 1108 (class 1255 OID 37190)
-- Name: userlocations_change_location(integer, text, text); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_change_location(location_id integer, location_name text, location_extent text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
 ret_id integer;
BEGIN
 update sys_scheme.map_extents 
  set name=location_name,extent=location_extent
  where id=location_id
  returning id into ret_id;
RETURN ret_id;
END;
$$;


--
-- TOC entry 1109 (class 1255 OID 37191)
-- Name: userlocations_delete_location(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_delete_location(location_id integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
 ret_id integer;
BEGIN
 delete from sys_scheme.map_extents 
  where id=location_id
  returning id into ret_id;
RETURN ret_id;
END;
$$;


--
-- TOC entry 1110 (class 1255 OID 37192)
-- Name: userlocations_drop_location_from_user(integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_drop_location_from_user(user_id_val integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
BEGIN
 return sys_scheme.userlocations_edit_params(user_id_val,null);
END;
$$;


--
-- TOC entry 1111 (class 1255 OID 37193)
-- Name: userlocations_edit_params(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_edit_params(user_id_val integer, location_id_val integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
 param_name text;
 ret_id integer;
BEGIN
 select id into ret_id from sys_scheme.user_db where id=user_id_val;
 if (ret_id is null) then
  return null;--такого юзера нетy
 end if;
 param_name:='id_map_extents';
 select id_user into ret_id 
  from sys_scheme.user_params
  where id_user=user_id_val and name=param_name;
 if (ret_id is null and location_id_val is null) then 
  return null;--нет такого параметра у этого пользователя, и нет значения для вставки
 end if;
 if (ret_id is null) then
  select id into ret_id from sys_scheme.map_extents where id=location_id_val;
  if (ret_id is null) then
   return null;--такой локации нету
  end if;
  INSERT INTO sys_scheme.user_params(name, id_user, "values")
   VALUES (param_name, user_id_val, array[location_id_val])
   returning id_user into ret_id;
  return ret_id;
 end if;
 if (location_id_val is null) then
  delete from sys_scheme.user_params
   where name=param_name and id_user=user_id_val
   returning id_user into ret_id;
  return ret_id;
 end if;
 select id into ret_id from sys_scheme.map_extents where id=location_id_val;
 if (ret_id is null) then
  return null;--такой локации нету
 end if;
 update sys_scheme.user_params 
  set "values"=array[location_id_val]
  where name=param_name and id_user=user_id_val
  returning id_user into ret_id;
 return ret_id;
END;
$$;


--
-- TOC entry 178 (class 1259 OID 37194)
-- Name: map_extents; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE map_extents (
    id integer NOT NULL,
    name character varying NOT NULL,
    extent character varying NOT NULL
);


--
-- TOC entry 1074 (class 1255 OID 37200)
-- Name: userlocations_get_locations(); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_get_locations() RETURNS SETOF map_extents
    LANGUAGE plpgsql
    AS $$
DECLARE
 r sys_scheme.map_extents%rowtype;
BEGIN
FOR r IN SELECT * FROM sys_scheme.map_extents LOOP
  RETURN NEXT r; -- return current row of SELECT
END LOOP;
RETURN;
END;
$$;


--
-- TOC entry 1038 (class 1255 OID 37201)
-- Name: userlocations_set_location_for_user(integer, integer); Type: FUNCTION; Schema: sys_scheme; Owner: -
--

CREATE FUNCTION userlocations_set_location_for_user(user_id_val integer, location_id_val integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
BEGIN
 if (location_id_val is null) then
  return null;
 end if;
 return sys_scheme.userlocations_edit_params(user_id_val,location_id_val);
END;
$$;


--
-- TOC entry 1666 (class 1255 OID 37202)
-- Name: concat(text); Type: AGGREGATE; Schema: sys_scheme; Owner: -
--

CREATE AGGREGATE concat(text) (
    SFUNC = textcat,
    STYPE = text,
    INITCOND = ''
);


--
-- TOC entry 179 (class 1259 OID 37203)
-- Name: action; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE action (
    id integer NOT NULL,
    name character varying NOT NULL,
    table_data boolean DEFAULT false NOT NULL,
    name_visible character varying NOT NULL,
    operation boolean DEFAULT false NOT NULL
);


--
-- TOC entry 180 (class 1259 OID 37211)
-- Name: action_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE action_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3495 (class 0 OID 0)
-- Dependencies: 180
-- Name: action_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE action_id_seq OWNED BY action.id;


--
-- TOC entry 181 (class 1259 OID 37213)
-- Name: actions_users; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE actions_users (
    id integer NOT NULL,
    id_action integer NOT NULL,
    id_user integer NOT NULL,
    read boolean DEFAULT false NOT NULL,
    write boolean DEFAULT false NOT NULL
);


--
-- TOC entry 182 (class 1259 OID 37218)
-- Name: actions_users_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE actions_users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3496 (class 0 OID 0)
-- Dependencies: 182
-- Name: actions_users_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE actions_users_id_seq OWNED BY actions_users.id;


--
-- TOC entry 183 (class 1259 OID 37220)
-- Name: db_version; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE db_version (
    major integer NOT NULL,
    minor integer NOT NULL,
    build integer NOT NULL,
    revision integer NOT NULL,
    version_seq integer,
    date_update timestamp without time zone DEFAULT now()
);


--
-- TOC entry 184 (class 1259 OID 37223)
-- Name: journal; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE journal (
    status integer NOT NULL,
    cl_id integer NOT NULL,
    id_version integer NOT NULL,
    date timestamp with time zone DEFAULT now(),
    id integer NOT NULL
);


--
-- TOC entry 185 (class 1259 OID 37227)
-- Name: journal_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE journal_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3497 (class 0 OID 0)
-- Dependencies: 185
-- Name: journal_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE journal_id_seq OWNED BY journal.id;


--
-- TOC entry 186 (class 1259 OID 37229)
-- Name: map_extents_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE map_extents_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3498 (class 0 OID 0)
-- Dependencies: 186
-- Name: map_extents_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE map_extents_id_seq OWNED BY map_extents.id;


--
-- TOC entry 229 (class 1259 OID 37624)
-- Name: sets_styles; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE sets_styles (
    id integer NOT NULL,
    name text NOT NULL,
    owner_set integer NOT NULL,
    show_set boolean
);


--
-- TOC entry 228 (class 1259 OID 37622)
-- Name: sets_styles_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE sets_styles_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3499 (class 0 OID 0)
-- Dependencies: 228
-- Name: sets_styles_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE sets_styles_id_seq OWNED BY sets_styles.id;


--
-- TOC entry 187 (class 1259 OID 37231)
-- Name: status; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE status (
    status character varying(10),
    message character varying(100),
    "time" timestamp without time zone,
    type integer
);


--
-- TOC entry 188 (class 1259 OID 37234)
-- Name: table_field_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_field_info (
    id integer NOT NULL,
    id_table integer NOT NULL,
    name_db character varying NOT NULL,
    name_map character varying NOT NULL,
    type_field integer NOT NULL,
    visible boolean DEFAULT false NOT NULL,
    name_lable character varying,
    is_reference boolean DEFAULT false NOT NULL,
    is_interval boolean DEFAULT false NOT NULL,
    is_style boolean DEFAULT false NOT NULL,
    ref_table integer,
    ref_field integer,
    ref_field_end integer,
    ref_field_name integer,
    num_order integer,
    read_only boolean DEFAULT false
);


--
-- TOC entry 3500 (class 0 OID 0)
-- Dependencies: 188
-- Name: TABLE table_field_info; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE table_field_info IS 'Информация по полям';


--
-- TOC entry 189 (class 1259 OID 37245)
-- Name: table_field_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_field_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3501 (class 0 OID 0)
-- Dependencies: 189
-- Name: table_field_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_field_info_id_seq OWNED BY table_field_info.id;


--
-- TOC entry 190 (class 1259 OID 37247)
-- Name: table_filtr_field_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_filtr_field_info (
    id integer NOT NULL,
    id_table integer NOT NULL,
    id_field integer NOT NULL,
    tip_operator integer NOT NULL
);


--
-- TOC entry 191 (class 1259 OID 37250)
-- Name: table_filtr_field_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_filtr_field_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3502 (class 0 OID 0)
-- Dependencies: 191
-- Name: table_filtr_field_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_filtr_field_info_id_seq OWNED BY table_filtr_field_info.id;


--
-- TOC entry 192 (class 1259 OID 37252)
-- Name: table_filtr_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_filtr_info (
    id integer NOT NULL,
    id_table integer NOT NULL,
    name character varying NOT NULL
);


--
-- TOC entry 193 (class 1259 OID 37258)
-- Name: table_filtr_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_filtr_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3503 (class 0 OID 0)
-- Dependencies: 193
-- Name: table_filtr_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_filtr_info_id_seq OWNED BY table_filtr_info.id;


--
-- TOC entry 194 (class 1259 OID 37260)
-- Name: table_filtr_tip_operator; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_filtr_tip_operator (
    id integer NOT NULL,
    name character varying NOT NULL,
    pered character varying DEFAULT ''::character varying NOT NULL,
    posle character varying DEFAULT ''::character varying NOT NULL
);


--
-- TOC entry 195 (class 1259 OID 37268)
-- Name: table_filtr_tip_operator_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_filtr_tip_operator_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3504 (class 0 OID 0)
-- Dependencies: 195
-- Name: table_filtr_tip_operator_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_filtr_tip_operator_id_seq OWNED BY table_filtr_tip_operator.id;


--
-- TOC entry 196 (class 1259 OID 37270)
-- Name: table_groups_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_groups_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3505 (class 0 OID 0)
-- Dependencies: 196
-- Name: table_groups_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_groups_id_seq OWNED BY table_groups.id;


--
-- TOC entry 197 (class 1259 OID 37272)
-- Name: table_groups_table; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_groups_table (
    id_table integer NOT NULL,
    id_group integer NOT NULL,
    order_num integer DEFAULT 1
);


--
-- TOC entry 198 (class 1259 OID 37276)
-- Name: table_history_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_history_info (
    id_table integer NOT NULL,
    history_table_name character varying NOT NULL,
    is_work boolean DEFAULT true NOT NULL,
    id_history_table integer NOT NULL,
    dataupd timestamp with time zone DEFAULT now() NOT NULL
);


--
-- TOC entry 199 (class 1259 OID 37284)
-- Name: table_history_info_id_history_table_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_history_info_id_history_table_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3506 (class 0 OID 0)
-- Dependencies: 199
-- Name: table_history_info_id_history_table_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_history_info_id_history_table_seq OWNED BY table_history_info.id_history_table;


--
-- TOC entry 200 (class 1259 OID 37286)
-- Name: table_history_photo; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_history_photo (
    id integer NOT NULL,
    id_table_photo integer NOT NULL,
    id_photo integer NOT NULL,
    id_obj integer NOT NULL,
    type_operation integer NOT NULL,
    user_name character varying NOT NULL,
    dataupdate timestamp with time zone DEFAULT now() NOT NULL,
    file bytea,
    img_preview bytea,
    file_name character varying,
    is_photo boolean,
    master_id integer
);


--
-- TOC entry 201 (class 1259 OID 37293)
-- Name: table_history_photo_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_history_photo_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3507 (class 0 OID 0)
-- Dependencies: 201
-- Name: table_history_photo_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_history_photo_id_seq OWNED BY table_history_photo.id;


--
-- TOC entry 202 (class 1259 OID 37295)
-- Name: table_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3508 (class 0 OID 0)
-- Dependencies: 202
-- Name: table_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_info_id_seq OWNED BY table_info.id;


--
-- TOC entry 230 (class 1259 OID 37638)
-- Name: table_info_sets; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_info_sets (
    id_table integer NOT NULL,
    id_set integer NOT NULL,
    scheme_name character varying,
    name_db character varying NOT NULL,
    name_map character varying NOT NULL,
    lablefiled character varying,
    map_style boolean DEFAULT false NOT NULL,
    geom_field character varying DEFAULT 'geom'::character varying NOT NULL,
    style_field character varying DEFAULT 'style'::character varying NOT NULL,
    geom_type integer NOT NULL,
    type integer NOT NULL,
    default_style boolean DEFAULT true NOT NULL,
    fontname character varying DEFAULT 'Map Symbols'::character varying,
    fontcolor integer DEFAULT 16711680,
    fontframecolor integer DEFAULT 16711680,
    fontsize integer DEFAULT 12,
    symbol integer DEFAULT 35,
    pencolor integer DEFAULT 16711680,
    pentype integer DEFAULT 2,
    penwidth integer DEFAULT 1,
    brushbgcolor bigint DEFAULT 16711680,
    brushfgcolor integer DEFAULT 16711680,
    brushstyle integer DEFAULT 0,
    brushhatch integer DEFAULT 1,
    read_only boolean DEFAULT false NOT NULL,
    photo boolean DEFAULT false,
    id_style integer DEFAULT 0,
    pk_fileld character varying DEFAULT 'id'::character varying NOT NULL,
    is_style boolean DEFAULT false,
    source_layer boolean DEFAULT false,
    image_column character varying DEFAULT ''::character varying,
    angle_column character varying DEFAULT ''::character varying,
    use_bounds boolean DEFAULT false,
    min_scale integer DEFAULT 0,
    max_scale integer DEFAULT 0,
    id_group integer DEFAULT 0 NOT NULL,
    default_visibl boolean DEFAULT false,
    view_name character varying,
    order_num integer DEFAULT 0,
    sql_view_string character varying,
    masterdb_history_id integer,
    connection_string text,
    remote_lgn text,
    remote_pwd text,
    fixed_history_id integer,
    range_colors boolean DEFAULT false,
    range_column character varying,
    precision_point integer DEFAULT 1,
    type_color integer DEFAULT 0,
    min_color bigint DEFAULT 0,
    min_val integer,
    max_color bigint DEFAULT 0,
    max_val integer,
    use_min_val boolean DEFAULT false,
    null_color bigint DEFAULT 0,
    use_null_color boolean DEFAULT false,
    hidden boolean DEFAULT false NOT NULL,
    use_max_val boolean DEFAULT false,
    label_showframe boolean DEFAULT false NOT NULL,
    label_framecolor integer DEFAULT 0 NOT NULL,
    label_parallel boolean DEFAULT true NOT NULL,
    label_overlap boolean DEFAULT true NOT NULL,
    label_usebounds boolean DEFAULT false NOT NULL,
    label_minscale integer DEFAULT 0 NOT NULL,
    label_maxscale integer DEFAULT 0 NOT NULL,
    label_offset integer DEFAULT 0 NOT NULL,
    label_graphicunits boolean DEFAULT false NOT NULL,
    label_fontname character varying DEFAULT 'Times New Roman'::character varying NOT NULL,
    label_fontcolor integer DEFAULT 0 NOT NULL,
    label_fontsize integer DEFAULT 9 NOT NULL,
    label_fontstrikeout boolean DEFAULT false NOT NULL,
    label_fontitalic boolean DEFAULT false NOT NULL,
    label_fontunderline boolean DEFAULT false NOT NULL,
    label_fontbold boolean DEFAULT false NOT NULL,
    label_uselabelstyle boolean DEFAULT false NOT NULL,
    label_showlabel boolean DEFAULT true NOT NULL,
    min_object_size integer DEFAULT 0 NOT NULL,
    ref_table integer,
    graphic_units boolean DEFAULT false,
    display_when_opening boolean DEFAULT true NOT NULL
);


--
-- TOC entry 231 (class 1259 OID 37721)
-- Name: table_order_set; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_order_set (
    id_set integer NOT NULL,
    id_table integer NOT NULL,
    order_num integer NOT NULL
);


--
-- TOC entry 203 (class 1259 OID 37297)
-- Name: table_photo_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_photo_info (
    id integer NOT NULL,
    id_table integer NOT NULL,
    photo_table character varying NOT NULL,
    photo_field character varying,
    photo_file character varying NOT NULL,
    id_field_tble character varying NOT NULL,
    masterdb_history_id integer,
    fixed_history_id integer
);


--
-- TOC entry 204 (class 1259 OID 37303)
-- Name: table_photo_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_photo_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3509 (class 0 OID 0)
-- Dependencies: 204
-- Name: table_photo_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_photo_info_id_seq OWNED BY table_photo_info.id;


--
-- TOC entry 205 (class 1259 OID 37305)
-- Name: table_right_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_right_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3510 (class 0 OID 0)
-- Dependencies: 205
-- Name: table_right_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_right_id_seq OWNED BY table_right.id;


--
-- TOC entry 206 (class 1259 OID 37307)
-- Name: table_schems; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_schems (
    id integer NOT NULL,
    name character varying NOT NULL
);


--
-- TOC entry 207 (class 1259 OID 37313)
-- Name: table_schems_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_schems_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3511 (class 0 OID 0)
-- Dependencies: 207
-- Name: table_schems_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_schems_id_seq OWNED BY table_schems.id;


--
-- TOC entry 208 (class 1259 OID 37315)
-- Name: table_style_info; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_style_info (
    id integer NOT NULL,
    tip_geom integer NOT NULL,
    fontname character varying DEFAULT 'Map Symbols'::character varying,
    fontcolor integer DEFAULT 16711680,
    fontframecolor integer DEFAULT 16711680,
    fontsize integer DEFAULT 12,
    symbol integer DEFAULT 35,
    pencolor integer DEFAULT 16711680,
    pentype integer DEFAULT 2,
    penwidth integer DEFAULT 1,
    brushbgcolor integer DEFAULT 16711680,
    brushfgcolor integer DEFAULT 16711680,
    brushstyle integer DEFAULT 0,
    brushhatch integer DEFAULT 1
);


--
-- TOC entry 209 (class 1259 OID 37333)
-- Name: table_style_info_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_style_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3512 (class 0 OID 0)
-- Dependencies: 209
-- Name: table_style_info_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_style_info_id_seq OWNED BY table_style_info.id;


--
-- TOC entry 210 (class 1259 OID 37335)
-- Name: table_type; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_type (
    id integer NOT NULL,
    name character varying NOT NULL,
    name_db character varying
);


--
-- TOC entry 3513 (class 0 OID 0)
-- Dependencies: 210
-- Name: TABLE table_type; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE table_type IS 'Типы данных полей';


--
-- TOC entry 211 (class 1259 OID 37341)
-- Name: table_type_geom; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE table_type_geom (
    id integer NOT NULL,
    name character varying NOT NULL,
    namedb character varying
);


--
-- TOC entry 3514 (class 0 OID 0)
-- Dependencies: 211
-- Name: TABLE table_type_geom; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE table_type_geom IS 'Типы геометрий';


--
-- TOC entry 212 (class 1259 OID 37347)
-- Name: table_type_geom_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_type_geom_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3515 (class 0 OID 0)
-- Dependencies: 212
-- Name: table_type_geom_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_type_geom_id_seq OWNED BY table_type_geom.id;


--
-- TOC entry 213 (class 1259 OID 37349)
-- Name: table_type_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3516 (class 0 OID 0)
-- Dependencies: 213
-- Name: table_type_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_type_id_seq OWNED BY table_type.id;


--
-- TOC entry 214 (class 1259 OID 37351)
-- Name: table_type_table_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE table_type_table_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3517 (class 0 OID 0)
-- Dependencies: 214
-- Name: table_type_table_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE table_type_table_id_seq OWNED BY table_type_table.id;


--
-- TOC entry 215 (class 1259 OID 37353)
-- Name: tokens; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE tokens (
    time_token timestamp without time zone NOT NULL
);


--
-- TOC entry 216 (class 1259 OID 37356)
-- Name: typ_users; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE typ_users (
    id_user integer NOT NULL,
    typ character varying,
    type_name character varying
);


--
-- TOC entry 217 (class 1259 OID 37362)
-- Name: typ_version; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE typ_version (
    id integer NOT NULL,
    name character varying
);


--
-- TOC entry 218 (class 1259 OID 37368)
-- Name: typ_version_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE typ_version_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3518 (class 0 OID 0)
-- Dependencies: 218
-- Name: typ_version_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE typ_version_id_seq OWNED BY typ_version.id;


--
-- TOC entry 219 (class 1259 OID 37370)
-- Name: upd_files; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE upd_files (
    name text NOT NULL,
    fileid oid NOT NULL,
    creation_date date DEFAULT now(),
    file_type character varying NOT NULL,
    user_type character varying,
    version character varying,
    file_length integer NOT NULL,
    start_app boolean,
    is_del boolean DEFAULT false NOT NULL
);


--
-- TOC entry 3519 (class 0 OID 0)
-- Dependencies: 219
-- Name: COLUMN upd_files.is_del; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON COLUMN upd_files.is_del IS '������� �� �������� ����� � ��������� ������ ������������';


--
-- TOC entry 220 (class 1259 OID 37378)
-- Name: upd_journal; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE upd_journal (
    user_login character varying NOT NULL,
    fileid oid NOT NULL,
    date_time timestamp without time zone NOT NULL,
    action character varying NOT NULL,
    file_name character varying
);


--
-- TOC entry 221 (class 1259 OID 37384)
-- Name: upd_updater; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE upd_updater (
    name text NOT NULL,
    fileid oid NOT NULL,
    creation_date date DEFAULT now(),
    file_length integer NOT NULL
);


--
-- TOC entry 222 (class 1259 OID 37391)
-- Name: upd_user_files; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE upd_user_files (
    name text NOT NULL,
    fileid oid NOT NULL,
    creation_date date NOT NULL,
    file_type character varying NOT NULL,
    user_id integer,
    file_length integer NOT NULL,
    id integer NOT NULL
);


--
-- TOC entry 223 (class 1259 OID 37397)
-- Name: upd_user_files_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE upd_user_files_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3520 (class 0 OID 0)
-- Dependencies: 223
-- Name: upd_user_files_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE upd_user_files_id_seq OWNED BY upd_user_files.id;


--
-- TOC entry 224 (class 1259 OID 37399)
-- Name: user_db_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE user_db_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3521 (class 0 OID 0)
-- Dependencies: 224
-- Name: user_db_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE user_db_id_seq OWNED BY user_db.id;


--
-- TOC entry 225 (class 1259 OID 37401)
-- Name: user_params; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE user_params (
    name text NOT NULL,
    id_user integer NOT NULL,
    "values" text[]
);


--
-- TOC entry 226 (class 1259 OID 37407)
-- Name: version; Type: TABLE; Schema: sys_scheme; Owner: -; Tablespace: 
--

CREATE TABLE version (
    id integer NOT NULL,
    prioritet boolean DEFAULT true NOT NULL,
    version_name character varying(100) NOT NULL,
    file bytea,
    size character varying,
    typ integer
);


--
-- TOC entry 3522 (class 0 OID 0)
-- Dependencies: 226
-- Name: TABLE version; Type: COMMENT; Schema: sys_scheme; Owner: -
--

COMMENT ON TABLE version IS 'Версии обновлений';


--
-- TOC entry 227 (class 1259 OID 37414)
-- Name: version_id_seq; Type: SEQUENCE; Schema: sys_scheme; Owner: -
--

CREATE SEQUENCE version_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 3523 (class 0 OID 0)
-- Dependencies: 227
-- Name: version_id_seq; Type: SEQUENCE OWNED BY; Schema: sys_scheme; Owner: -
--

ALTER SEQUENCE version_id_seq OWNED BY version.id;


--
-- TOC entry 3160 (class 2604 OID 37416)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY action ALTER COLUMN id SET DEFAULT nextval('action_id_seq'::regclass);


--
-- TOC entry 3163 (class 2604 OID 37417)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY actions_users ALTER COLUMN id SET DEFAULT nextval('actions_users_id_seq'::regclass);


--
-- TOC entry 3166 (class 2604 OID 37418)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY journal ALTER COLUMN id SET DEFAULT nextval('journal_id_seq'::regclass);


--
-- TOC entry 3157 (class 2604 OID 37419)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY map_extents ALTER COLUMN id SET DEFAULT nextval('map_extents_id_seq'::regclass);


--
-- TOC entry 3208 (class 2604 OID 37627)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY sets_styles ALTER COLUMN id SET DEFAULT nextval('sets_styles_id_seq'::regclass);


--
-- TOC entry 3172 (class 2604 OID 37420)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_field_info ALTER COLUMN id SET DEFAULT nextval('table_field_info_id_seq'::regclass);


--
-- TOC entry 3173 (class 2604 OID 37421)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_filtr_field_info ALTER COLUMN id SET DEFAULT nextval('table_filtr_field_info_id_seq'::regclass);


--
-- TOC entry 3174 (class 2604 OID 37422)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_filtr_info ALTER COLUMN id SET DEFAULT nextval('table_filtr_info_id_seq'::regclass);


--
-- TOC entry 3177 (class 2604 OID 37423)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_filtr_tip_operator ALTER COLUMN id SET DEFAULT nextval('table_filtr_tip_operator_id_seq'::regclass);


--
-- TOC entry 3083 (class 2604 OID 37424)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_groups ALTER COLUMN id SET DEFAULT nextval('table_groups_id_seq'::regclass);


--
-- TOC entry 3181 (class 2604 OID 37425)
-- Name: id_history_table; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_history_info ALTER COLUMN id_history_table SET DEFAULT nextval('table_history_info_id_history_table_seq'::regclass);


--
-- TOC entry 3183 (class 2604 OID 37426)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_history_photo ALTER COLUMN id SET DEFAULT nextval('table_history_photo_id_seq'::regclass);


--
-- TOC entry 3144 (class 2604 OID 37427)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info ALTER COLUMN id SET DEFAULT nextval('table_info_id_seq'::regclass);


--
-- TOC entry 3184 (class 2604 OID 37428)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_photo_info ALTER COLUMN id SET DEFAULT nextval('table_photo_info_id_seq'::regclass);


--
-- TOC entry 3156 (class 2604 OID 37429)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_right ALTER COLUMN id SET DEFAULT nextval('table_right_id_seq'::regclass);


--
-- TOC entry 3185 (class 2604 OID 37430)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_schems ALTER COLUMN id SET DEFAULT nextval('table_schems_id_seq'::regclass);


--
-- TOC entry 3198 (class 2604 OID 37431)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_style_info ALTER COLUMN id SET DEFAULT nextval('table_style_info_id_seq'::regclass);


--
-- TOC entry 3199 (class 2604 OID 37432)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_type ALTER COLUMN id SET DEFAULT nextval('table_type_id_seq'::regclass);


--
-- TOC entry 3200 (class 2604 OID 37433)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_type_geom ALTER COLUMN id SET DEFAULT nextval('table_type_geom_id_seq'::regclass);


--
-- TOC entry 3147 (class 2604 OID 37434)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_type_table ALTER COLUMN id SET DEFAULT nextval('table_type_table_id_seq'::regclass);


--
-- TOC entry 3201 (class 2604 OID 37435)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY typ_version ALTER COLUMN id SET DEFAULT nextval('typ_version_id_seq'::regclass);


--
-- TOC entry 3205 (class 2604 OID 37436)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY upd_user_files ALTER COLUMN id SET DEFAULT nextval('upd_user_files_id_seq'::regclass);


--
-- TOC entry 3153 (class 2604 OID 37437)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY user_db ALTER COLUMN id SET DEFAULT nextval('user_db_id_seq'::regclass);


--
-- TOC entry 3207 (class 2604 OID 37438)
-- Name: id; Type: DEFAULT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY version ALTER COLUMN id SET DEFAULT nextval('version_id_seq'::regclass);


--
-- TOC entry 3386 (class 0 OID 37203)
-- Dependencies: 179
-- Data for Name: action; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3524 (class 0 OID 0)
-- Dependencies: 180
-- Name: action_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('action_id_seq', 1, false);


--
-- TOC entry 3388 (class 0 OID 37213)
-- Dependencies: 181
-- Data for Name: actions_users; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3525 (class 0 OID 0)
-- Dependencies: 182
-- Name: actions_users_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('actions_users_id_seq', 1, false);


--
-- TOC entry 3390 (class 0 OID 37220)
-- Dependencies: 183
-- Data for Name: db_version; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO db_version (major, minor, build, revision, version_seq, date_update) VALUES (2, 8, 0, 0, 1, now());


--
-- TOC entry 3391 (class 0 OID 37223)
-- Dependencies: 184
-- Data for Name: journal; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3526 (class 0 OID 0)
-- Dependencies: 185
-- Name: journal_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('journal_id_seq', 1, false);


--
-- TOC entry 3385 (class 0 OID 37194)
-- Dependencies: 178
-- Data for Name: map_extents; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3527 (class 0 OID 0)
-- Dependencies: 186
-- Name: map_extents_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('map_extents_id_seq', 1, false);


--
-- TOC entry 3436 (class 0 OID 37624)
-- Dependencies: 229
-- Data for Name: sets_styles; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3528 (class 0 OID 0)
-- Dependencies: 228
-- Name: sets_styles_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('sets_styles_id_seq', 1, false);


--
-- TOC entry 3394 (class 0 OID 37231)
-- Dependencies: 187
-- Data for Name: status; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3395 (class 0 OID 37234)
-- Dependencies: 188
-- Data for Name: table_field_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3529 (class 0 OID 0)
-- Dependencies: 189
-- Name: table_field_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_field_info_id_seq', 1, false);


--
-- TOC entry 3397 (class 0 OID 37247)
-- Dependencies: 190
-- Data for Name: table_filtr_field_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3530 (class 0 OID 0)
-- Dependencies: 191
-- Name: table_filtr_field_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_filtr_field_info_id_seq', 1, false);


--
-- TOC entry 3399 (class 0 OID 37252)
-- Dependencies: 192
-- Data for Name: table_filtr_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3531 (class 0 OID 0)
-- Dependencies: 193
-- Name: table_filtr_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_filtr_info_id_seq', 1, false);


--
-- TOC entry 3401 (class 0 OID 37260)
-- Dependencies: 194
-- Data for Name: table_filtr_tip_operator; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (1, '=', '', '');
INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (2, '>', '', '');
INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (3, '<', '', '');
INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (4, '<>', '', '');
INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (5, 'like', '''''''', '''''''');
INSERT INTO table_filtr_tip_operator (id, name, pered, posle) VALUES (6, 'like %%', '''''%''', '''%''''');


--
-- TOC entry 3532 (class 0 OID 0)
-- Dependencies: 195
-- Name: table_filtr_tip_operator_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_filtr_tip_operator_id_seq', 1, false);


--
-- TOC entry 3380 (class 0 OID 37026)
-- Dependencies: 170
-- Data for Name: table_groups; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3533 (class 0 OID 0)
-- Dependencies: 196
-- Name: table_groups_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_groups_id_seq', 1, false);


--
-- TOC entry 3404 (class 0 OID 37272)
-- Dependencies: 197
-- Data for Name: table_groups_table; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3405 (class 0 OID 37276)
-- Dependencies: 198
-- Data for Name: table_history_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3534 (class 0 OID 0)
-- Dependencies: 199
-- Name: table_history_info_id_history_table_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_history_info_id_history_table_seq', 1, false);


--
-- TOC entry 3407 (class 0 OID 37286)
-- Dependencies: 200
-- Data for Name: table_history_photo; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3535 (class 0 OID 0)
-- Dependencies: 201
-- Name: table_history_photo_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_history_photo_id_seq', 1, false);


--
-- TOC entry 3381 (class 0 OID 37051)
-- Dependencies: 171
-- Data for Name: table_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3536 (class 0 OID 0)
-- Dependencies: 202
-- Name: table_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_info_id_seq', 1, false);


--
-- TOC entry 3437 (class 0 OID 37638)
-- Dependencies: 230
-- Data for Name: table_info_sets; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3438 (class 0 OID 37721)
-- Dependencies: 231
-- Data for Name: table_order_set; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3410 (class 0 OID 37297)
-- Dependencies: 203
-- Data for Name: table_photo_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3537 (class 0 OID 0)
-- Dependencies: 204
-- Name: table_photo_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_photo_info_id_seq', 1, false);


--
-- TOC entry 3384 (class 0 OID 37150)
-- Dependencies: 176
-- Data for Name: table_right; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3538 (class 0 OID 0)
-- Dependencies: 205
-- Name: table_right_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_right_id_seq', 1, false);


--
-- TOC entry 3413 (class 0 OID 37307)
-- Dependencies: 206
-- Data for Name: table_schems; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3539 (class 0 OID 0)
-- Dependencies: 207
-- Name: table_schems_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_schems_id_seq', 2, true);


--
-- TOC entry 3415 (class 0 OID 37315)
-- Dependencies: 208
-- Data for Name: table_style_info; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3540 (class 0 OID 0)
-- Dependencies: 209
-- Name: table_style_info_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_style_info_id_seq', 1, false);


--
-- TOC entry 3417 (class 0 OID 37335)
-- Dependencies: 210
-- Data for Name: table_type; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO table_type (id, name, name_db) VALUES (1, 'Целое', 'INTEGER');
INSERT INTO table_type (id, name, name_db) VALUES (2, 'Текст', 'character varying');
INSERT INTO table_type (id, name, name_db) VALUES (3, 'Дата', 'date');
INSERT INTO table_type (id, name, name_db) VALUES (4, 'Дата и время', 'timestamp with time zone');
INSERT INTO table_type (id, name, name_db) VALUES (5, 'Геометрия', 'geometry');
INSERT INTO table_type (id, name, name_db) VALUES (6, 'Вещественное', 'numeric');


--
-- TOC entry 3418 (class 0 OID 37341)
-- Dependencies: 211
-- Data for Name: table_type_geom; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO table_type_geom (id, name, namedb) VALUES (0, 'Без геометрии', NULL);
INSERT INTO table_type_geom (id, name, namedb) VALUES (1, 'Точки', 'MULTIPOINT');
INSERT INTO table_type_geom (id, name, namedb) VALUES (2, 'Линии', 'MULTILINESTRING');
INSERT INTO table_type_geom (id, name, namedb) VALUES (3, 'Площадные объекты', 'MULTIPOLYGON');


--
-- TOC entry 3541 (class 0 OID 0)
-- Dependencies: 212
-- Name: table_type_geom_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_type_geom_id_seq', 1, false);


--
-- TOC entry 3542 (class 0 OID 0)
-- Dependencies: 213
-- Name: table_type_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_type_id_seq', 1, false);


--
-- TOC entry 3382 (class 0 OID 37117)
-- Dependencies: 172
-- Data for Name: table_type_table; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO table_type_table (id, name, map_layer) VALUES (1, 'Слой карты', true);
INSERT INTO table_type_table (id, name, map_layer) VALUES (2, 'Справочник', false);
INSERT INTO table_type_table (id, name, map_layer) VALUES (3, 'Интервал', false);
INSERT INTO table_type_table (id, name, map_layer) VALUES (4, 'Таблица с данными', false);


--
-- TOC entry 3543 (class 0 OID 0)
-- Dependencies: 214
-- Name: table_type_table_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('table_type_table_id_seq', 1, false);


--
-- TOC entry 3422 (class 0 OID 37353)
-- Dependencies: 215
-- Data for Name: tokens; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO tokens (time_token) VALUES ('2011-05-24 17:04:28');
INSERT INTO tokens (time_token) VALUES ('2011-05-24 17:26:12');
INSERT INTO tokens (time_token) VALUES ('2011-05-26 12:17:59');


--
-- TOC entry 3423 (class 0 OID 37356)
-- Dependencies: 216
-- Data for Name: typ_users; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO typ_users (id_user, typ, type_name) VALUES (1, 'admin', 'Администратор');
INSERT INTO typ_users (id_user, typ, type_name) VALUES (2, 'user', 'Пользователь');


--
-- TOC entry 3424 (class 0 OID 37362)
-- Dependencies: 217
-- Data for Name: typ_version; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--

INSERT INTO typ_version (id, name) VALUES (1, 'Для администраторов');
INSERT INTO typ_version (id, name) VALUES (2, 'Для пользователей');
INSERT INTO typ_version (id, name) VALUES (3, 'Для всех');


--
-- TOC entry 3544 (class 0 OID 0)
-- Dependencies: 218
-- Name: typ_version_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('typ_version_id_seq', 1, false);


--
-- TOC entry 3426 (class 0 OID 37370)
-- Dependencies: 219
-- Data for Name: upd_files; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3427 (class 0 OID 37378)
-- Dependencies: 220
-- Data for Name: upd_journal; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3428 (class 0 OID 37384)
-- Dependencies: 221
-- Data for Name: upd_updater; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3429 (class 0 OID 37391)
-- Dependencies: 222
-- Data for Name: upd_user_files; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3545 (class 0 OID 0)
-- Dependencies: 223
-- Name: upd_user_files_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('upd_user_files_id_seq', 1, true);


--
-- TOC entry 3383 (class 0 OID 37134)
-- Dependencies: 174
-- Data for Name: user_db; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3546 (class 0 OID 0)
-- Dependencies: 224
-- Name: user_db_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('user_db_id_seq', 3, true);


--
-- TOC entry 3432 (class 0 OID 37401)
-- Dependencies: 225
-- Data for Name: user_params; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3433 (class 0 OID 37407)
-- Dependencies: 226
-- Data for Name: version; Type: TABLE DATA; Schema: sys_scheme; Owner: -
--



--
-- TOC entry 3547 (class 0 OID 0)
-- Dependencies: 227
-- Name: version_id_seq; Type: SEQUENCE SET; Schema: sys_scheme; Owner: -
--

SELECT pg_catalog.setval('version_id_seq', 1, false);


--
-- TOC entry 3291 (class 2606 OID 37440)
-- Name: db_version_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY db_version
    ADD CONSTRAINT db_version_pkey PRIMARY KEY (major, minor, build, revision);


--
-- TOC entry 3337 (class 2606 OID 37442)
-- Name: id; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY version
    ADD CONSTRAINT id PRIMARY KEY (id);


--
-- TOC entry 3293 (class 2606 OID 37444)
-- Name: journal_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY journal
    ADD CONSTRAINT journal_pkey PRIMARY KEY (cl_id, id_version);


--
-- TOC entry 3287 (class 2606 OID 37446)
-- Name: mapsurfer_map_extents_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY map_extents
    ADD CONSTRAINT mapsurfer_map_extents_pkey PRIMARY KEY (id);


--
-- TOC entry 3327 (class 2606 OID 37448)
-- Name: pk_upd_files; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY upd_files
    ADD CONSTRAINT pk_upd_files PRIMARY KEY (name, file_type);


--
-- TOC entry 3289 (class 2606 OID 37450)
-- Name: pkey_actions; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY action
    ADD CONSTRAINT pkey_actions PRIMARY KEY (id);


--
-- TOC entry 3283 (class 2606 OID 37452)
-- Name: pkey_user; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY user_db
    ADD CONSTRAINT pkey_user PRIMARY KEY (id);


--
-- TOC entry 3339 (class 2606 OID 37632)
-- Name: sets_styles_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY sets_styles
    ADD CONSTRAINT sets_styles_pkey PRIMARY KEY (id);


--
-- TOC entry 3295 (class 2606 OID 37454)
-- Name: table_field_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_field_info
    ADD CONSTRAINT table_field_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3297 (class 2606 OID 37456)
-- Name: table_filtr_field_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_filtr_field_info
    ADD CONSTRAINT table_filtr_field_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3299 (class 2606 OID 37458)
-- Name: table_filtr_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_filtr_info
    ADD CONSTRAINT table_filtr_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3301 (class 2606 OID 37460)
-- Name: table_filtr_tip_operator_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_filtr_tip_operator
    ADD CONSTRAINT table_filtr_tip_operator_pkey PRIMARY KEY (id);


--
-- TOC entry 3271 (class 2606 OID 37462)
-- Name: table_group_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_groups
    ADD CONSTRAINT table_group_pkey PRIMARY KEY (id);


--
-- TOC entry 3303 (class 2606 OID 37464)
-- Name: table_groups_table_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_groups_table
    ADD CONSTRAINT table_groups_table_pkey PRIMARY KEY (id_table, id_group);


--
-- TOC entry 3305 (class 2606 OID 37466)
-- Name: table_history_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_history_info
    ADD CONSTRAINT table_history_info_pkey PRIMARY KEY (id_table, dataupd);


--
-- TOC entry 3307 (class 2606 OID 37468)
-- Name: table_history_photo_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_history_photo
    ADD CONSTRAINT table_history_photo_pkey PRIMARY KEY (id);


--
-- TOC entry 3273 (class 2606 OID 37470)
-- Name: table_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3275 (class 2606 OID 37472)
-- Name: table_info_scheme_name_name_db_key; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_scheme_name_name_db_key UNIQUE (scheme_name, name_db);


--
-- TOC entry 3277 (class 2606 OID 37474)
-- Name: table_info_scheme_name_name_db_key1; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_scheme_name_name_db_key1 UNIQUE (scheme_name, name_db);


--
-- TOC entry 3279 (class 2606 OID 37476)
-- Name: table_info_scheme_name_name_db_key2; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_scheme_name_name_db_key2 UNIQUE (scheme_name, name_db);


--
-- TOC entry 3341 (class 2606 OID 37705)
-- Name: table_info_sets_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_info_sets
    ADD CONSTRAINT table_info_sets_pkey PRIMARY KEY (id_table, id_set);


--
-- TOC entry 3343 (class 2606 OID 37725)
-- Name: table_order_set_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_order_set
    ADD CONSTRAINT table_order_set_pkey PRIMARY KEY (id_set, id_table);


--
-- TOC entry 3309 (class 2606 OID 37478)
-- Name: table_photo_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_photo_info
    ADD CONSTRAINT table_photo_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3285 (class 2606 OID 37480)
-- Name: table_right_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_right
    ADD CONSTRAINT table_right_pkey PRIMARY KEY (id);


--
-- TOC entry 3311 (class 2606 OID 37482)
-- Name: table_schems_name_key; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_schems
    ADD CONSTRAINT table_schems_name_key UNIQUE (name);


--
-- TOC entry 3313 (class 2606 OID 37484)
-- Name: table_schems_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_schems
    ADD CONSTRAINT table_schems_pkey PRIMARY KEY (id);


--
-- TOC entry 3315 (class 2606 OID 37486)
-- Name: table_style_info_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_style_info
    ADD CONSTRAINT table_style_info_pkey PRIMARY KEY (id);


--
-- TOC entry 3319 (class 2606 OID 37488)
-- Name: table_type_geom_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_type_geom
    ADD CONSTRAINT table_type_geom_pkey PRIMARY KEY (id);


--
-- TOC entry 3317 (class 2606 OID 37490)
-- Name: table_type_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_type
    ADD CONSTRAINT table_type_pkey PRIMARY KEY (id);


--
-- TOC entry 3281 (class 2606 OID 37492)
-- Name: table_type_table_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY table_type_table
    ADD CONSTRAINT table_type_table_pkey PRIMARY KEY (id);


--
-- TOC entry 3321 (class 2606 OID 37494)
-- Name: tokens_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY tokens
    ADD CONSTRAINT tokens_pkey PRIMARY KEY (time_token);


--
-- TOC entry 3323 (class 2606 OID 37496)
-- Name: typ_users_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY typ_users
    ADD CONSTRAINT typ_users_pkey PRIMARY KEY (id_user);


--
-- TOC entry 3325 (class 2606 OID 37498)
-- Name: typ_version_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY typ_version
    ADD CONSTRAINT typ_version_pkey PRIMARY KEY (id);


--
-- TOC entry 3329 (class 2606 OID 37500)
-- Name: upd_updater_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY upd_updater
    ADD CONSTRAINT upd_updater_pkey PRIMARY KEY (name);


--
-- TOC entry 3331 (class 2606 OID 37502)
-- Name: upd_user_files_name_file_type_user_id_key; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY upd_user_files
    ADD CONSTRAINT upd_user_files_name_file_type_user_id_key UNIQUE (name, file_type, user_id);


--
-- TOC entry 3333 (class 2606 OID 37504)
-- Name: upd_user_files_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY upd_user_files
    ADD CONSTRAINT upd_user_files_pkey PRIMARY KEY (id);


--
-- TOC entry 3335 (class 2606 OID 37506)
-- Name: user_params_pkey; Type: CONSTRAINT; Schema: sys_scheme; Owner: -; Tablespace: 
--

ALTER TABLE ONLY user_params
    ADD CONSTRAINT user_params_pkey PRIMARY KEY (name, id_user);


--
-- TOC entry 3373 (class 2620 OID 37507)
-- Name: del_after_field; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER del_after_field AFTER DELETE ON table_field_info FOR EACH ROW EXECUTE PROCEDURE del_field();


--
-- TOC entry 3371 (class 2620 OID 37508)
-- Name: delete_old_user; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER delete_old_user BEFORE DELETE ON user_db FOR EACH ROW EXECUTE PROCEDURE delete_user();


--
-- TOC entry 3370 (class 2620 OID 37509)
-- Name: ins_table_for_admin; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER ins_table_for_admin AFTER INSERT ON table_info FOR EACH ROW EXECUTE PROCEDURE insrt_new_table();


--
-- TOC entry 3372 (class 2620 OID 37510)
-- Name: ins_upd_new_right; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER ins_upd_new_right BEFORE INSERT OR UPDATE ON table_right FOR EACH ROW EXECUTE PROCEDURE set_right_to_table();


--
-- TOC entry 3374 (class 2620 OID 37511)
-- Name: insrt_after_field; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER insrt_after_field AFTER INSERT OR UPDATE ON table_field_info FOR EACH ROW EXECUTE PROCEDURE insert_after_field();


--
-- TOC entry 3375 (class 2620 OID 37512)
-- Name: insrt_field; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER insrt_field BEFORE INSERT ON table_field_info FOR EACH ROW EXECUTE PROCEDURE insrt_new_field();


--
-- TOC entry 3376 (class 2620 OID 37513)
-- Name: insrt_table_in_group; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER insrt_table_in_group BEFORE INSERT ON table_groups_table FOR EACH ROW EXECUTE PROCEDURE insrt_new_table_in_group();


--
-- TOC entry 3378 (class 2620 OID 37514)
-- Name: lo_unlink_before_delete; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER lo_unlink_before_delete BEFORE DELETE ON upd_updater FOR EACH ROW EXECUTE PROCEDURE unlink_before_delete();


--
-- TOC entry 3377 (class 2620 OID 37515)
-- Name: lo_unlink_before_delete; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER lo_unlink_before_delete BEFORE DELETE ON upd_files FOR EACH ROW EXECUTE PROCEDURE unlink_before_delete();


--
-- TOC entry 3379 (class 2620 OID 37516)
-- Name: lo_unlink_before_delete; Type: TRIGGER; Schema: sys_scheme; Owner: -
--

CREATE TRIGGER lo_unlink_before_delete BEFORE DELETE ON upd_user_files FOR EACH ROW EXECUTE PROCEDURE unlink_before_delete();


--
-- TOC entry 3349 (class 2606 OID 37517)
-- Name: actions_users_id_action_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY actions_users
    ADD CONSTRAINT actions_users_id_action_fkey FOREIGN KEY (id_action) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3350 (class 2606 OID 37522)
-- Name: actions_users_id_user_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY actions_users
    ADD CONSTRAINT actions_users_id_user_fkey FOREIGN KEY (id_user) REFERENCES user_db(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3351 (class 2606 OID 37527)
-- Name: journal_cl_id_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY journal
    ADD CONSTRAINT journal_cl_id_fkey FOREIGN KEY (cl_id) REFERENCES user_db(id);


--
-- TOC entry 3352 (class 2606 OID 37532)
-- Name: journal_id_version_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY journal
    ADD CONSTRAINT journal_id_version_fkey FOREIGN KEY (id_version) REFERENCES version(id);


--
-- TOC entry 3364 (class 2606 OID 37633)
-- Name: sets_styles_owner_set_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY sets_styles
    ADD CONSTRAINT sets_styles_owner_set_fkey FOREIGN KEY (owner_set) REFERENCES user_db(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3353 (class 2606 OID 37537)
-- Name: table_field_info_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_field_info
    ADD CONSTRAINT table_field_info_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3354 (class 2606 OID 37542)
-- Name: table_field_info_type_field_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_field_info
    ADD CONSTRAINT table_field_info_type_field_fkey FOREIGN KEY (type_field) REFERENCES table_type(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3355 (class 2606 OID 37547)
-- Name: table_filtr_field_info_id_field_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_filtr_field_info
    ADD CONSTRAINT table_filtr_field_info_id_field_fkey FOREIGN KEY (id_field) REFERENCES table_field_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3356 (class 2606 OID 37552)
-- Name: table_filtr_field_info_tip_operator_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_filtr_field_info
    ADD CONSTRAINT table_filtr_field_info_tip_operator_fkey FOREIGN KEY (tip_operator) REFERENCES table_filtr_tip_operator(id);


--
-- TOC entry 3357 (class 2606 OID 37557)
-- Name: table_groups_table_id_group_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_groups_table
    ADD CONSTRAINT table_groups_table_id_group_fkey FOREIGN KEY (id_group) REFERENCES table_groups(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3358 (class 2606 OID 37562)
-- Name: table_groups_table_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_groups_table
    ADD CONSTRAINT table_groups_table_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3359 (class 2606 OID 37567)
-- Name: table_history_info_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_history_info
    ADD CONSTRAINT table_history_info_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3344 (class 2606 OID 37572)
-- Name: table_info_geom_type_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_geom_type_fkey FOREIGN KEY (geom_type) REFERENCES table_type_geom(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3365 (class 2606 OID 37706)
-- Name: table_info_sets_geom_type_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info_sets
    ADD CONSTRAINT table_info_sets_geom_type_fkey FOREIGN KEY (geom_type) REFERENCES table_type_geom(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3366 (class 2606 OID 37711)
-- Name: table_info_sets_id_set_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info_sets
    ADD CONSTRAINT table_info_sets_id_set_fkey FOREIGN KEY (id_set) REFERENCES sets_styles(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3367 (class 2606 OID 37716)
-- Name: table_info_sets_type_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info_sets
    ADD CONSTRAINT table_info_sets_type_fkey FOREIGN KEY (type) REFERENCES table_type_table(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3345 (class 2606 OID 37577)
-- Name: table_info_type_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_info
    ADD CONSTRAINT table_info_type_fkey FOREIGN KEY (type) REFERENCES table_type_table(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3368 (class 2606 OID 37726)
-- Name: table_order_set_id_set_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_order_set
    ADD CONSTRAINT table_order_set_id_set_fkey FOREIGN KEY (id_set) REFERENCES sets_styles(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3369 (class 2606 OID 37731)
-- Name: table_order_set_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_order_set
    ADD CONSTRAINT table_order_set_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3360 (class 2606 OID 37582)
-- Name: table_photo_info_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_photo_info
    ADD CONSTRAINT table_photo_info_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3347 (class 2606 OID 37587)
-- Name: table_right_id_table_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_right
    ADD CONSTRAINT table_right_id_table_fkey FOREIGN KEY (id_table) REFERENCES table_info(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3348 (class 2606 OID 37592)
-- Name: table_right_id_user_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY table_right
    ADD CONSTRAINT table_right_id_user_fkey FOREIGN KEY (id_user) REFERENCES user_db(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3361 (class 2606 OID 37597)
-- Name: upd_user_files_user_id_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY upd_user_files
    ADD CONSTRAINT upd_user_files_user_id_fkey FOREIGN KEY (user_id) REFERENCES user_db(id) ON DELETE CASCADE;


--
-- TOC entry 3346 (class 2606 OID 37602)
-- Name: user_db_typ_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY user_db
    ADD CONSTRAINT user_db_typ_fkey FOREIGN KEY (typ) REFERENCES typ_users(id_user) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3362 (class 2606 OID 37607)
-- Name: user_params_id_user_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY user_params
    ADD CONSTRAINT user_params_id_user_fkey FOREIGN KEY (id_user) REFERENCES user_db(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3363 (class 2606 OID 37612)
-- Name: version_typ_fkey; Type: FK CONSTRAINT; Schema: sys_scheme; Owner: -
--

ALTER TABLE ONLY version
    ADD CONSTRAINT version_typ_fkey FOREIGN KEY (typ) REFERENCES typ_version(id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2013-04-01 16:10:39

--
-- PostgreSQL database dump complete
--

