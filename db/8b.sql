DROP FUNCTION sys_scheme.create_user(character varying, character varying, character varying, character varying, character varying, integer);

CREATE OR REPLACE FUNCTION sys_scheme.create_user(
    user_login character varying,
    user_pass character varying,
    user_pass_sync character varying,
    user_name_full character varying,
    user_otdel character varying,
    user_window_name character varying,
    user_typ integer)
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
				EXECUTE 'CREATE ROLE "' || user_login || '" LOGIN PASSWORD ''' || lower(md5(user_pass)) || ''' NOSUPERUSER INHERIT CREATEDB CREATEROLE;';
				EXECUTE 'GRANT '|| sys_scheme.get_admin_group() ||' TO "' || user_login ||'"';
			END IF;
			IF user_typ=2 THEN
				is_admin = FALSE;
				EXECUTE 'CREATE ROLE "' || user_login || '" LOGIN PASSWORD ''' || lower(md5(user_pass)) || ''' NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE';
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
  
CREATE OR REPLACE FUNCTION sys_scheme.user_change_pw(
    user_id integer,
    user_pass character varying,
    user_pass_sync character varying)
  RETURNS boolean AS
$BODY$DECLARE
	user_login text;
BEGIN
	SELECT "login" INTO user_login FROM sys_scheme.user_db WHERE id = user_id;
	IF (user_login is NULL) THEN
		RAISE EXCEPTION '|%|Такого пользователя не существует',12;
	END IF;
	EXECUTE 'ALTER Role "' || user_login || '" PASSWORD ''' || lower(md5(user_pass)) || ''';';
	
	UPDATE sys_scheme.user_db SET pass = user_pass_sync WHERE id = user_id;
	return TRUE;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;