CREATE OR REPLACE FUNCTION sys_scheme.create_exists_table_get_info(scheme_name_db character varying, table_name_db character varying, table_name_map character varying, pk_field character varying, geom_field_f character varying) RETURNS integer AS
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
		WHERE table_schema like scheme_name_db AND table_name like table_name_db ORDER BY ordinal_position
	LOOP
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

BEGIN
sql_string:='';
select_string:='SELECT ';
index_tab_alias:=1;
SELECT scheme_name, name_db, lablefiled INTO scheme_name_val, table_name_val, label_string FROM sys_scheme.table_info WHERE id = id_table_val;

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

select_string:=select_string||'(SELECT ('||label_string||') FROM "'||scheme_name_val||'"."'||table_name_val||'" tforlabel WHERE tforlabel.gid = "'||scheme_name_val||'"."'||table_name_val||'"."gid" ) as "_ResultLabel_"';
--select_string:=substring(select_string from 0 for char_length(select_string)-1);
sql_string:=select_string||' '||from_string;
RETURN sql_string;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;


ALTER TABLE sys_scheme.user_db
  ADD UNIQUE (login);

CREATE AGGREGATE sys_scheme.array_accum (anyelement)
(
	sfunc = array_append,
	stype = anyarray,
	initcond = '{}'
);


DROP FUNCTION sys_scheme.get_style_list(integer);
DROP TYPE sys_scheme.style_info;

CREATE TYPE sys_scheme.style_info AS
   (scheme character varying,
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
    style_name text,
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
    use_max_val boolean);


CREATE OR REPLACE FUNCTION sys_scheme.get_style_list(id_table_val integer)
  RETURNS SETOF sys_scheme.style_info AS
$BODY$DECLARE
r RECORD;
style RECORD;
temp_val sys_scheme.style_info;
is_reference_val boolean;
type_field_val integer;
sql_string character varying;
ref_table_val integer;
ref_field_val integer;
ref_field_name_val integer;
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
				SELECT ref_field_name INTO ref_field_name_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
				sql_string:='SELECT "'||sys_scheme.get_table_field_name(ref_field_val)||'" as ref_field, 
					"'||sys_scheme.get_table_field_name(ref_field_name_val)||'" as ref_field_name, fontname, fontcolor, 
				       fontframecolor, fontsize, symbol, pencolor, pentype, penwidth, 
				       brushbgcolor, brushfgcolor, brushstyle, brushhatch FROM "'||sys_scheme.get_table_scheme_name(ref_table_val)||'"."'||
				       sys_scheme.get_table_name(ref_table_val)||'"';
				raise notice 'SQL1:: %', sql_string;
				FOR style IN EXECUTE sql_string LOOP
					temp_val.reference_int_val=style.ref_field;
					temp_val.style_name = style.ref_field_name;
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
					SELECT ref_field_name INTO ref_field_name_val FROM sys_scheme.table_field_info WHERE id_table = id_table_val AND is_style = TRUE;
					sql_string:='SELECT "'||sys_scheme.get_table_field_name(ref_field_val)||'" as ref_field, 
						"'||sys_scheme.get_table_field_name(ref_field_end_val)||'" as ref_field_end,
						"'||sys_scheme.get_table_field_name(ref_field_name_val)||'" as ref_field_name, 
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
						temp_val.style_name = style.ref_field_name;
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
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100
  ROWS 1000;

CREATE OR REPLACE FUNCTION sys_scheme.update_db_from_2_9_0_0_to_2_9_1_0()
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
	build_val_new:=1;
	revision_val_new:=0;
	
	version_seq_new:=4;
	
	major_val_old:=2;
	minor_val_old:=9;
	build_val_old:=0;
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
			RAISE EXCEPTION 'Сообщение: %', 'Необходимо сначало обновить до версии 2.9.0!';
			return false;
		END IF;
		
		raise notice 'Сообщение: %', 'Фиксируем факт обновления';
		INSERT INTO sys_scheme.db_version(major, minor, build, revision, version_seq) VALUES (major_val_new, minor_val_new, build_val_new, revision_val_new, version_seq_new);
		
		raise notice 'Сообщение: %', 'Обновление прошло успешно!';
		return true;
END;$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;

SELECT sys_scheme.update_db_from_2_9_0_0_to_2_9_1_0();
  -- запуск фикс ДБ  
SELECT sys_scheme.super_fix_db();