CREATE TABLE sys_scheme.table_view_info
(
  id serial NOT NULL,
  table_id integer NOT NULL,
  user_id integer NOT NULL,
  title character varying NOT NULL,
  is_default boolean NOT NULL DEFAULT false,
  CONSTRAINT table_view_pkey PRIMARY KEY (id),
  CONSTRAINT table_view_table_id_fkey FOREIGN KEY (table_id)
      REFERENCES sys_scheme.table_info (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT table_view_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES sys_scheme.user_db (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);

CREATE TABLE sys_scheme.table_view_field_info
(
  id serial NOT NULL,
  view_id integer NOT NULL,
  field_id integer NOT NULL,
  order_number integer NOT NULL,
  is_visible boolean NOT NULL DEFAULT true,
  CONSTRAINT table_view_field_info_pkey PRIMARY KEY (id),
  CONSTRAINT table_view_field_info_field_id_fkey FOREIGN KEY (field_id)
      REFERENCES sys_scheme.table_field_info (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT table_view_field_info_view_id_fkey FOREIGN KEY (view_id)
      REFERENCES sys_scheme.table_view_info (id) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);


DROP FUNCTION sys_scheme.get_sql_for_table_new(integer);


CREATE OR REPLACE FUNCTION sys_scheme.create_user(user_login character varying, user_pass character varying, user_pass_sync character varying, user_name_full character varying, user_otdel character varying, user_window_name character varying, user_typ integer)
  RETURNS integer AS
$BODY$DECLARE
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
				EXECUTE 'CREATE ROLE "' || user_login || '" LOGIN ENCRYPTED PASSWORD ''' || user_pass || ''' NOSUPERUSER INHERIT CREATEDB CREATEROLE;';
				EXECUTE 'GRANT '|| sys_scheme.get_admin_group() ||' TO "' || user_login ||'"';
			END IF;
			IF user_typ=2 THEN
				is_admin = FALSE;
				EXECUTE 'CREATE ROLE "' || user_login || '" LOGIN ENCRYPTED PASSWORD ''' || user_pass || ''' NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE';
			END IF;
			--
			SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_client_group()) INTO exists_group;
			IF exists_group=FALSE THEN
				PERFORM sys_scheme.create_client_group();
			END IF;
			EXECUTE 'GRANT '|| sys_scheme.get_client_group() ||' TO "' ||  user_login||'"';
		
			SELECT nextval('sys_scheme.user_db_id_seq') INTO id_new_user;
			INSERT INTO sys_scheme.user_db (id, "name", pass, name_full, "login", otdel,"admin", window_name, typ) 
				VALUES (id_new_user, '', user_pass_sync, user_name_full, user_login, user_otdel, is_admin, user_window_name, user_typ);
	END IF;
RETURN id_new_user;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.add_right_to_table(scheme character varying, table_name character varying, user_id integer, right_type character varying, val boolean)
  RETURNS boolean AS
$BODY$DECLARE
pk_field character varying;
user_name character varying;
--exists_photo boolean;
BEGIN
	SELECT ti.pk_fileld INTO pk_field FROM sys_scheme.table_info ti WHERE ti.scheme_name = scheme AND ti.name_db = table_name;
	--SELECT ti.photo INTO exists_photo FROM sys_scheme.table_info ti WHERE ti.scheme_name = scheme AND ti.name_db = table_name;
	SELECT ud."login" INTO user_name FROM sys_scheme.user_db ud WHERE ud.id = user_id;
	IF val=true THEN
		EXECUTE 'GRANT '||right_type||' ON '||scheme||'."'||table_name||'" TO "' || user_name||'"';
	END IF;
	IF val=false THEN
		EXECUTE 'REVOKE '||right_type||' ON '||scheme||'."'||table_name||'" FROM "' || user_name||'"';
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
			EXECUTE 'REVOKE '||sys_scheme.get_admin_group()||' FROM "'||user_login||'";';
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO "'||user_login||'";';
			EXECUTE 'ALTER ROLE "'||user_login||'" NOCREATEDB NOCREATEROLE;';
			UPDATE sys_scheme.user_db SET typ=2, "admin" = FALSE WHERE id = $1;
			RETURN TRUE;
		END IF;
	ELSE
		IF $2=1 THEN
			EXECUTE 'GRANT '||sys_scheme.get_admin_group()||' TO "'||user_login||'";';
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO "'||user_login||'";';
			EXECUTE 'ALTER ROLE "'||user_login||'" CREATEDB CREATEROLE;';
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
	EXECUTE 'GRANT ALL ON sys_scheme.table_view_info TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_view_field_info TO ' || admin_group_name;

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
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_view_field_info_id_seq TO ' || admin_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_view_info_id_seq TO ' || admin_group_name;
	
	RETURN true;
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
	EXECUTE 'GRANT ALL ON sys_scheme.table_view_info TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON sys_scheme.table_view_field_info TO ' || client_group_name;

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
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_view_field_info_id_seq TO ' || client_group_name;
	EXECUTE 'GRANT ALL ON TABLE sys_scheme.table_view_info_id_seq TO ' || client_group_name;
	RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.fix_admin_user_for_groups()
  RETURNS boolean AS
$BODY$DECLARE
rec RECORD;
BEGIN
	FOR rec in SELECT id, "login" FROM sys_scheme.user_db WHERE typ=1 LOOP
		BEGIN
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO "'||rec."login"||'" ;';
			EXECUTE 'GRANT '||sys_scheme.get_admin_group()||' TO "'||rec."login"||'" ;';
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.fix_client_user_for_groups()
  RETURNS boolean AS
$BODY$DECLARE
rec RECORD;
BEGIN
	FOR rec in SELECT id, "login" FROM sys_scheme.user_db LOOP
		BEGIN
			EXECUTE 'GRANT '||sys_scheme.get_client_group()||' TO "'||rec."login"||'";';
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.fix_table_grants()
  RETURNS boolean AS
$BODY$DECLARE
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
					EXECUTE 'REVOKE ALL ON '||full_name_table||' FROM "' || u_rec."login"||'";';
					EXECUTE 'GRANT SELECT ON '||full_name_table||' TO "' || u_rec."login"||'";';
					IF exists_photo=true THEN
						EXECUTE 'GRANT SELECT ON "'||sceme_name_table||'"."'||name_table_photo||'" TO "' || u_rec."login"||'";';
						EXECUTE 'GRANT ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" TO "' || u_rec."login"||'";';
					END IF;
				END IF;
				IF (r_rec.read_data=true AND r_rec.write_data=true) or (r_rec.read_data=false AND r_rec.write_data=true) THEN
					EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||full_name_table||' TO ' || u_rec."login";
	raise notice 'GRANT:: %', 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||full_name_table||' TO "' || u_rec."login"||'";';
					IF seq_name is not null AND seq_name not like '' THEN
						EXECUTE 'GRANT ALL ON '||seq_name||' TO "' || u_rec."login"||'";';
					END IF;
					IF exists_photo=true THEN
						EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON "'||sceme_name_table||'"."'||name_table_photo||'" TO "' || u_rec."login"||'";';
						EXECUTE 'GRANT ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" TO "' || u_rec."login"||'";';
					END IF;
				END IF;
				IF (r_rec.read_data=false AND r_rec.write_data=false) THEN
					EXECUTE 'REVOKE ALL ON '||full_name_table||' FROM "' || u_rec."login"||'";';
					IF seq_name is not null AND seq_name not like '' THEN
						EXECUTE 'REVOKE ALL ON '||seq_name||' FROM "' || u_rec."login"||'";';
					END IF;
					IF exists_photo=true THEN
						EXECUTE 'REVOKE SELECT, UPDATE, INSERT, DELETE ON "'||sceme_name_table||'"."'||name_table_photo||'" FROM "' || u_rec."login"||'";';
						EXECUTE 'REVOKE ALL ON "'||sceme_name_table||'"."'||name_table_photo||'_id_seq" FROM "' || u_rec."login"||'";';
					END IF;
				END IF;
			END LOOP;
		EXCEPTION
			WHEN OTHERS THEN
			RAISE NOTICE 'ERROR: %', SQLERRM;
		END;
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
CREATE OR REPLACE FUNCTION sys_scheme.user_change_login(user_id integer, new_login character varying)
  RETURNS boolean AS
$BODY$DECLARE
	user_login text;
BEGIN
	SELECT "login" INTO user_login FROM sys_scheme.user_db WHERE id = user_id;
	IF (user_login is NULL) THEN
		RAISE EXCEPTION '|%|Такого пользователя не существует',12;
	END IF;
	
	EXECUTE 'ALTER ROLE "' || user_login || '" RENAME TO "' || new_login || '";';
	
	UPDATE sys_scheme.user_db SET "login" = new_login WHERE id = user_id;
	return TRUE;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.user_change_pw(user_id integer, user_pass character varying, user_pass_sync character varying)
  RETURNS boolean AS
$BODY$DECLARE
	user_login text;
BEGIN
	SELECT "login" INTO user_login FROM sys_scheme.user_db WHERE id = user_id;
	IF (user_login is NULL) THEN
		RAISE EXCEPTION '|%|Такого пользователя не существует',12;
	END IF;
	EXECUTE 'ALTER Role "' || user_login || '" ENCRYPTED PASSWORD ''' || user_pass || ''';';
	
	UPDATE sys_scheme.user_db SET pass = user_pass_sync WHERE id = user_id;
	return TRUE;
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
	EXECUTE 'REVOKE '||sys_scheme.get_client_group()||' FROM "'||login_user||'";';
	EXECUTE 'REVOKE '||sys_scheme.get_admin_group()||' FROM "'||login_user||'";';
	PERFORM  sys_scheme.user_grant_clear(login_user);
	EXECUTE 'DROP USER "'|| login_user||'";';
	RETURN OLD;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.insrt_user()
  RETURNS trigger AS
$BODY$DECLARE
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
		EXECUTE 'CREATE ROLE "' || login_user || '" LOGIN ENCRYPTED PASSWORD ''' || pass_user || ''' NOSUPERUSER INHERIT CREATEDB CREATEROLE;';
		EXECUTE 'GRANT '|| sys_scheme.get_admin_group() ||' TO "' || login_user||'";';
	END IF;
	IF NEW.typ=2 THEN
		NEW."admin" = FALSE;
		EXECUTE 'CREATE ROLE "' || login_user || '" LOGIN ENCRYPTED PASSWORD ''' || pass_user || ''' NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE';
	END IF;
	--
	SELECT EXISTS(SELECT true FROM pg_roles WHERE rolname like sys_scheme.get_client_group()) INTO exists_group;
	IF exists_group=FALSE THEN
		PERFORM sys_scheme.create_client_group();
	END IF;
	EXECUTE 'GRANT '|| sys_scheme.get_client_group() ||' TO "' ||  login_user||'";';
	RETURN NEW;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.set_right_to_table()
  RETURNS trigger AS
$BODY$DECLARE
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
	SELECT photo_table into photo_table_name FROM sys_scheme.table_photo_info where id_table=NEW.id_table;
	SELECT ud."login" INTO user_name FROM sys_scheme.user_db ud WHERE ud.id = NEW.id_user;
	SELECT EXISTS(SELECT true FROM information_schema.sequences WHERE sequence_schema like scheme_name_trg
								AND sequence_name like table_name||'_'||pk_field||'_seq') INTO exists_seq;
	IF NEW.read_data=true AND NEW.write_data=false THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'" FROM "' || user_name||'";';
		EXECUTE 'GRANT SELECT ON '||scheme_name_trg||'."'||table_name||'" TO "' || user_name||'";';
		IF exists_photo=true THEN
			EXECUTE 'GRANT SELECT ON '||scheme_name_trg||'."'||photo_table_name||'" TO "' || user_name||'";';
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" TO "' || user_name||'";';
		END IF;
	END IF;
	IF (NEW.read_data=true AND NEW.write_data=true) or (NEW.read_data=false AND NEW.write_data=true) THEN
		EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||table_name||'" TO "' || user_name||'";';
		IF exists_seq THEN
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||table_name||'_'||pk_field||'_seq" TO "' || user_name||'";';
		END IF;
		IF exists_photo=true THEN
			EXECUTE 'GRANT SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||photo_table_name||'" TO "' || user_name||'";';
			EXECUTE 'GRANT ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" TO "' || user_name||'";';
		END IF;
	END IF;
	IF (NEW.read_data=false AND NEW.write_data=false) THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'" FROM "' || user_name||'";';
		IF exists_seq THEN
		EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||table_name||'_'||pk_field||'_seq" FROM "' || user_name||'";';
		END IF;
		IF exists_photo=true THEN
			EXECUTE 'REVOKE SELECT, UPDATE, INSERT, DELETE ON '||scheme_name_trg||'."'||photo_table_name||'" FROM "' || user_name||'";';
			EXECUTE 'REVOKE ALL ON '||scheme_name_trg||'."'||photo_table_name||'_id_seq" FROM "' || user_name||'";';
		END IF;
	END IF;
RETURN NEW;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
  CREATE OR REPLACE FUNCTION sys_scheme.get_sql_for_table(id_table_val integer, id_filter_table integer, pk_value integer)
  RETURNS character varying AS
$BODY$DECLARE
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

pk_field_filter_name text;

BEGIN
select_id_string:=', ';
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name INTO scheme_name_val FROM sys_scheme.table_info WHERE id = id_table_val;

IF id_filter_table IS NOT NULL THEN
	SELECT pk_fileld INTO pk_field_filter_name FROM sys_scheme.table_info WHERE id = id_filter_table;
END IF; 

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

	    IF id_filter_table IS NOT NULL AND id_filter_table = rec_field.ref_table THEN
		from_string := from_string|| 'AND '||alias_string||'."'||pk_field_filter_name||'"='||pk_value||' ';
	    END IF;

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

END LOOP;

IF (char_length(select_id_string)>=2) THEN
    select_string:=substring(select_string from 0 for char_length(select_string)-1);
END IF;
select_id_string:=substring(select_id_string from 0 for char_length(select_id_string)-1);

sql_string:=select_string||select_id_string||' '||from_string;
RETURN sql_string;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sys_scheme.get_sql_for_table(integer, integer, integer)
  OWNER TO postgres;
COMMENT ON FUNCTION sys_scheme.get_sql_for_table(integer, integer, integer) IS 'Получение sql-запроса для создания Представления';



CREATE OR REPLACE FUNCTION sys_scheme.get_sql_for_table(id_table_val integer)
  RETURNS character varying AS
$BODY$DECLARE
BEGIN
RETURN sys_scheme.get_sql_for_table(id_table_val, null, null);
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
ALTER FUNCTION sys_scheme.get_sql_for_table(integer)
  OWNER TO postgres;
COMMENT ON FUNCTION sys_scheme.get_sql_for_table(integer) IS 'Получение sql-запроса для создания Представления';



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

	IF real_delete=TRUE THEN
		EXECUTE 'ALTER TABLE "'||table_shemename||'"."'||table_namedb||'" DROP COLUMN "'||field_namedb||'" CASCADE;';
		EXECUTE 'ALTER TABLE "'||table_shemename||'"."'||table_namedb||'" OWNER TO "' || sys_scheme.get_admin_group() ||'";';
	END IF;
	DELETE FROM sys_scheme.table_field_info WHERE id = field_id;
	
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_history;

	IF exists_history=TRUE THEN
		EXECUTE 'ALTER TABLE "'||table_shemename||'"."'||table_namedb||'_history" DROP COLUMN "'||field_namedb||'" CASCADE;';
		EXECUTE 'ALTER TABLE "'||table_shemename||'"."'||table_namedb||'_history" OWNER TO "' || sys_scheme.get_admin_group() ||'";';
	END IF;
	PERFORM sys_scheme.create_view_for_table(table_id);
	RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.del_field()
  RETURNS trigger AS
$BODY$DECLARE
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
	--PERFORM sys_scheme.create_view_for_table(OLD.id_table);
	RETURN NEW;
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
instead_trigger_sql text;
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
	instead_trigger_sql = sys_scheme.create_instead_trigger_for_view(id_table_val);
	IF instead_trigger_sql IS NOT NULL THEN
		EXECUTE instead_trigger_sql;
	END IF;
RETURN table_name_val||'_vw';
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
		IF __rec_field.name_db <> __pk_field THEN
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
		END IF;
	END LOOP;
	IF __insert_value='' THEN
		RETURN null;
	END IF;
	
	__sql_create_trigger:=
'CREATE OR REPLACE FUNCTION "'||__table_scheme||'"."'||__view_name||'_instead"()
RETURNS trigger AS
$BODY_FUNCTION$
DECLARE
pk_value integer;
BEGIN

IF TG_OP = ''INSERT'' THEN
	INSERT INTO "'||__table_scheme||'"."'||__table_name||'" (' || __insert_set || ') VALUES (' || __insert_value || ') returning "'||__pk_field||'" into pk_value;
	NEW."'||__pk_field||'" = pk_value;
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
  
CREATE OR REPLACE FUNCTION sys_scheme.delete_table(table_id integer, real_delete boolean)
  RETURNS boolean AS
$BODY$DECLARE
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
		EXECUTE 'DROP TABLE IF EXISTS "'||table_shemename||'"."photo_'||table_namedb||'" CASCADE;';
		--EXECUTE 'ALTER TABLE '||table_shemename||'.'||table_namedb||'_vw OWNER TO '||current_user||'';
		EXECUTE 'DROP VIEW IF EXISTS  "'||table_shemename||'"."'||table_namedb||'_vw";';
		EXECUTE 'DROP TABLE "'||table_shemename||'"."'||table_namedb||'";';

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
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_11_1_0_to_2_12_0_0()
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
	minor_val_new:=12;
	build_val_new:=0;
	revision_val_new:=0;
	
	version_seq_new:=8;
	
	major_val_old:=2;
	minor_val_old:=11;
	build_val_old:=1;
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
			RAISE EXCEPTION 'Сообщение: %', 'Необходимо сначало обновить до версии 2.11.1!';
			return false;
		END IF;
		
		raise notice 'Сообщение: %', 'Фиксируем факт обновления';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice 'Сообщение: %', 'Обновление прошло успешно!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_11_1_0_to_2_12_0_0();
  -- запуск фикс ДБ  
SELECT sys_scheme.super_fix_db();