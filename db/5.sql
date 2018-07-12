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
						AND (table_type like 'BASE TABLE' OR table_type like 'VIEW')) INTO exists_table_val;
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


CREATE OR REPLACE FUNCTION sys_scheme.create_field(table_id integer, field_name_db character varying, field_name_map character varying, field_type integer, field_name_lable character varying)
  RETURNS integer AS
$BODY$DECLARE
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

	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = table_id) INTO exists_history;
	IF exists_history=TRUE THEN
		EXECUTE sql_string_history;
		EXECUTE 'ALTER TABLE '||table_sheme_name||'.'||table_name_db||'_history OWNER TO ' || sys_scheme.get_admin_group() ||';';
	END IF;
	
	EXECUTE 'ALTER TABLE '||table_sheme_name||'.'||table_name_db||' OWNER TO ' || sys_scheme.get_admin_group() ||';';
	SELECT nextval('sys_scheme.table_field_info_id_seq') INTO id_new_field;
	INSERT INTO sys_scheme.table_field_info(id, id_table, name_db, name_map, type_field, visible, name_lable, is_reference, is_interval) 
	VALUES (id_new_field, table_id, field_name_db, field_name_map, field_type, true, field_name_lable, false, false);

RETURN id_new_field;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.create_sync_field(id_table_val integer)
  RETURNS character varying AS
$BODY$DECLARE
rec RECORD;
exists_field_val boolean;
exists_table_val boolean;
exists_history boolean;
seq_name text;
BEGIN
	SELECT EXISTS(SELECT 1 FROM sys_scheme.table_history_info WHERE id_table = id_table_val) INTO exists_history;
	
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
				IF exists_history=TRUE THEN
					EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||'_history ADD COLUMN "master_id" integer;';
					EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||'_history OWNER TO ' || sys_scheme.get_admin_group() ||';';
				END IF;
			END IF;

			SELECT EXISTS(SELECT true FROM information_schema.columns WHERE table_schema like rec.scheme_name 
									AND table_name like rec.name_db
									AND column_name like 'status') INTO exists_field_val;
			IF exists_field_val = FALSE THEN
				EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||' ADD COLUMN status integer;';
				IF exists_history=TRUE THEN
					EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||'_history ADD COLUMN "status" integer;';
					EXECUTE 'ALTER TABLE '||rec.scheme_name||'.'||rec.name_db||'_history OWNER TO ' || sys_scheme.get_admin_group() ||';';
				END IF;
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
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.get_sql_create_view(id_table_val integer)
  RETURNS character varying AS
$BODY$DECLARE
table_name_val character varying;
scheme_name_val character varying;

index_tab_alias integer;
alias_string character varying;
alias_string2 character varying;
sql_string character varying;
select_string character varying;
from_string character varying;

label_string character varying;

rec_field sys_scheme.table_field_info;
pk_field_val text;

BEGIN
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name, name_db, lablefiled, pk_fileld INTO scheme_name_val, table_name_val, label_string, pk_field_val FROM sys_scheme.table_info WHERE id = id_table_val;

if label_string is null THEN
	label_string:='null';
END IF;


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
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as "_RealValue_'||rec_field.name_db||'", ';
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
		select_string:=select_string ||'"'||scheme_name_val||'"."'||table_name_val||'"."'||rec_field.name_db||'" '|| 
						'as "_RealValue_'||rec_field.name_db||'", ';
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

select_string:=select_string||'(SELECT ('||label_string||') FROM "'||scheme_name_val||'"."'||table_name_val||'" tforlabel WHERE tforlabel."'||pk_field_val||'" = "'||scheme_name_val||'"."'||table_name_val||'"."'||pk_field_val||'" ) as "_ResultLabel_"';
--select_string:=substring(select_string from 0 for char_length(select_string)-1);
sql_string:=select_string||' '||from_string;
RETURN sql_string;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

CREATE OR REPLACE FUNCTION sys_scheme.insert_after_field()
  RETURNS trigger AS
$BODY$DECLARE
cnt integer;
BEGIN
	PERFORM sys_scheme.create_view_for_table(NEW.id_table);
	RETURN NEW;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.insrt_new_field()
  RETURNS trigger AS
$BODY$DECLARE
cnt integer;
BEGIN
	SELECT max(num_order)+1 INTO cnt FROM sys_scheme.table_field_info WHERE id_table = new.id_table AND num_order is not NULL;
	if cnt is not NULL THEN
		new.num_order = cnt;
	ELSE
		new.num_order = 0;
	END IF;
	PERFORM sys_scheme.super_fix_table_sync_fields(NEW.id_table);
	return NEW;
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
			WHERE id_table=table_id AND ref_table IS NOT NULL AND ref_table <> table_id)
		LOOP
			PERFORM sys_scheme.set_read_right(linked_id, user_id, read_val);
		END LOOP;
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


CREATE OR REPLACE FUNCTION sys_scheme.set_read_right(table_id integer, user_id integer, read_val boolean, depth_val integer)
  RETURNS boolean AS
$BODY$DECLARE
exists_item boolean;
linked_id INTEGER;
BEGIN
	IF depth_val>4 THEN
		RETURN true;
	END IF;
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
			PERFORM sys_scheme.set_read_right(linked_id, user_id, read_val, depth_val+1);
		END LOOP;
	END IF;

RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

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
			PERFORM sys_scheme.set_read_right(table_id, user_id, true, 0);
		END IF;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
  
CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_9_1_0_to_2_10_0_0()
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
	minor_val_new:=10;
	build_val_new:=0;
	revision_val_new:=0;
	
	version_seq_new:=5;
	
	major_val_old:=2;
	minor_val_old:=9;
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
			RAISE EXCEPTION 'Сообщение: %', 'Необходимо сначало обновить до версии 2.9.1!';
			return false;
		END IF;
		
		raise notice 'Сообщение: %', 'Фиксируем факт обновления';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice 'Сообщение: %', 'Обновление прошло успешно!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_9_1_0_to_2_10_0_0();
  -- запуск фикс ДБ  
SELECT sys_scheme.super_fix_db();