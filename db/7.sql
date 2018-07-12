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

if label_string is null OR label_string='' THEN
	label_string:='null::text';
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
  
CREATE OR REPLACE FUNCTION sys_scheme.fix_geomtry_colums()
  RETURNS boolean AS
$BODY$DECLARE
rec RECORD;
rec_srid RECORD;
exists_in_gc boolean;
type_geom_in_sys text;
exists_obj boolean;
BEGIN

	FOR rec in SELECT id, scheme_name, name_db, geom_type, geom_field FROM sys_scheme.table_info WHERE "type"=1 LOOP
		SELECT EXISTS(SELECT TRUE FROM geometry_columns WHERE f_table_schema like rec.scheme_name AND f_table_name like rec.name_db 
					AND f_geometry_column like rec.geom_field) INTO exists_in_gc;
raise notice '1:: %', exists_in_gc;
		
		exists_obj:=false;
		IF exists_in_gc=FALSE THEN
			SELECT namedb INTO type_geom_in_sys FROM sys_scheme.table_type_geom WHERE id =rec.geom_type; 
			FOR rec_srid IN EXECUTE 'SELECT st_srid('||rec.geom_field||')::INTEGER as srid_val, geometrytype('||rec.geom_field||')::text as geom_type
						FROM "'||rec.scheme_name||'"."'||rec.name_db||'" LIMIT 1' LOOP
raise notice '2:: %', rec_srid.srid_val;
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
		--EXECUTE '';
	END LOOP;
RETURN true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
  
CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_10_1_0_to_2_11_1_0()
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
	minor_val_new:=11;
	build_val_new:=1;
	revision_val_new:=0;
	
	version_seq_new:=7;
	
	major_val_old:=2;
	minor_val_old:=10;
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
			RAISE EXCEPTION 'Сообщение: %', 'Необходимо сначало обновить до версии 2.10.1!';
			return false;
		END IF;
		
		raise notice 'Сообщение: %', 'Фиксируем факт обновления';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice 'Сообщение: %', 'Обновление прошло успешно!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_10_1_0_to_2_11_1_0();
  -- запуск фикс ДБ  
SELECT sys_scheme.super_fix_db();