-- Project Name : 英作文練習
-- Date/Time    : 2025/08/19 3:24:17
-- RDBMS Type   : Microsoft SQL Server 2008 ～
-- Application  : A5:SQL Mk-2

/*
  << 注意！！ >>
  BackupToTempTable, RestoreFromTempTable疑似命令が付加されています。
  これにより、drop table, create table 後もデータが残ります。
  この機能は一時的に $$TableName のような一時テーブルを作成します。
  この機能は A5:SQL Mk-2でのみ有効であることに注意してください。
*/

-- フレーズ帳アイテム
--* RestoreFromTempTable
create table M_PHRASE_BOOK_ITEMS (
  phrase_book_id UNIQUEIDENTIFIER not null
  , phrase_id UNIQUEIDENTIFIER not null
  , order_no INT
  , note NVARCHAR(100)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index M_PHRASE_BOOK_ITEMS_IX1
  on M_PHRASE_BOOK_ITEMS(phrase_book_id,order_no);

create unique index M_PHRASE_BOOK_ITEMS_PKI
  on M_PHRASE_BOOK_ITEMS(phrase_book_id,phrase_id);

alter table M_PHRASE_BOOK_ITEMS
  add constraint M_PHRASE_BOOK_ITEMS_PKC primary key (phrase_book_id,phrase_id);

-- フレーズ帳マスタ
--* RestoreFromTempTable
create table M_PHRASE_BOOKS (
  phrase_book_id UNIQUEIDENTIFIER not null
  , phrase_book_name NVARCHAR(50) not null
  , description NVARCHAR(100)
  , user_id NVARCHAR(450)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create unique index M_PHRASE_BOOKS_IX1
  on M_PHRASE_BOOKS(user_id,phrase_book_name);

create unique index M_PHRASE_BOOKS_PKI
  on M_PHRASE_BOOKS(phrase_book_id);

alter table M_PHRASE_BOOKS
  add constraint M_PHRASE_BOOKS_PKC primary key (phrase_book_id);

-- 操作種別マスタ
--* RestoreFromTempTable
create table M_OPERATION_TYPES (
  operation_type_id UNIQUEIDENTIFIER not null
  , operation_type_code NVARCHAR(50) not null
  , operation_type_name NVARCHAR(20) not null
  , operation_type_limit INT not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

alter table M_OPERATION_TYPES add constraint M_OPERATION_TYPES_IX1
  unique (operation_type_name) ;

create unique index M_OPERATION_TYPES_PKI
  on M_OPERATION_TYPES(operation_type_id);

alter table M_OPERATION_TYPES
  add constraint M_OPERATION_TYPES_PKC primary key (operation_type_id);

-- 操作履歴
--* RestoreFromTempTable
create table D_DAILY_USAGE (
  user_id NVARCHAR(450) not null
  , operation_date DATE not null
  , operation_type_id UNIQUEIDENTIFIER not null
  , operation_count INT default 0 not null
) ;

create unique index D_DAILY_USAGE_PKI
  on D_DAILY_USAGE(user_id,operation_date,operation_type_id);

alter table D_DAILY_USAGE
  add constraint D_DAILY_USAGE_PKC primary key (user_id,operation_date,operation_type_id);

-- 格言マスタ
--* RestoreFromTempTable
create table M_PROVERBS (
  proverb_id UNIQUEIDENTIFIER not null
  , proverb_text NVARCHAR(200) not null
  , meaning NVARCHAR(200)
  , author NVARCHAR(100)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create unique index M_PROVERBS_PKI
  on M_PROVERBS(proverb_id);

alter table M_PROVERBS
  add constraint M_PROVERBS_PKC primary key (proverb_id);

-- 英語日記タグ
--* RestoreFromTempTable
create table D_ENGLISH_DIARY_TAGS (
  diary_id UNIQUEIDENTIFIER not null
  , diary_tag_id UNIQUEIDENTIFIER not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_ENGLISH_DIARY_TAGS_IX1
  on D_ENGLISH_DIARY_TAGS(diary_id);

create index D_ENGLISH_DIARY_TAGS_IX2
  on D_ENGLISH_DIARY_TAGS(diary_tag_id);

create unique index D_ENGLISH_DIARY_TAGS_PKI
  on D_ENGLISH_DIARY_TAGS(diary_id,diary_tag_id);

alter table D_ENGLISH_DIARY_TAGS
  add constraint D_ENGLISH_DIARY_TAGS_PKC primary key (diary_id,diary_tag_id);

-- 日記タグ
--* RestoreFromTempTable
create table M_DIARY_TAGS (
  tag_id UNIQUEIDENTIFIER not null
  , tag_name NVARCHAR(50)
  , user_id NVARCHAR(450)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create unique index M_DIARY_TAGS_IX1
  on M_DIARY_TAGS(user_id,tag_name);

create unique index M_DIARY_TAGS_PKI
  on M_DIARY_TAGS(tag_id);

alter table M_DIARY_TAGS
  add constraint M_DIARY_TAGS_PKC primary key (tag_id);

-- 英語日記
--* RestoreFromTempTable
create table D_ENGLISH_DIARYS (
  diary_id UNIQUEIDENTIFIER not null
  , title NVARCHAR(100) not null
  , content NVARCHAR(1000) not null
  , note NVARCHAR(1000)
  , correction NVARCHAR(1000)
  , explanation NVARCHAR(1000)
  , user_id NVARCHAR(450) not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_ENGLISH_DIARYS_IX1
  on D_ENGLISH_DIARYS(user_id,created_at);

create index D_ENGLISH_DIARYS_IX2
  on D_ENGLISH_DIARYS(user_id,title);

create unique index D_ENGLISH_DIARYS_PKI
  on D_ENGLISH_DIARYS(diary_id);

alter table D_ENGLISH_DIARYS
  add constraint D_ENGLISH_DIARYS_PKC primary key (diary_id);

-- 復習種別マスタ
--* RestoreFromTempTable
create table M_REVIEW_TYPES (
  review_type_id UNIQUEIDENTIFIER not null
  , review_type_name NVARCHAR(20) not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

alter table M_REVIEW_TYPES add constraint M_REVIEW_TYPES_IX1
  unique (review_type_name) ;

create unique index M_REVIEW_TYPES_PKI
  on M_REVIEW_TYPES(review_type_id);

alter table M_REVIEW_TYPES
  add constraint M_REVIEW_TYPES_PKC primary key (review_type_id);

-- 復習履歴
--* RestoreFromTempTable
create table D_REVIEW_LOGS (
  [review_id ] UNIQUEIDENTIFIER not null
  , phrase_id UNIQUEIDENTIFIER not null
  , review_date DATETIME default GETDATE() not null
  , review_type_id UNIQUEIDENTIFIER not null
  , test_id UNIQUEIDENTIFIER not null
  , test_result_detail_no INT not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_REVIEW_LOGS_IX1
  on D_REVIEW_LOGS(phrase_id,review_date);

create index D_REVIEW_LOGS_IX2
  on D_REVIEW_LOGS(test_id,test_result_detail_no);

create unique index D_REVIEW_LOGS_PKI
  on D_REVIEW_LOGS(review_id);

alter table D_REVIEW_LOGS
  add constraint D_REVIEW_LOGS_PKC primary key (review_id);

-- 成績マスタ
--* RestoreFromTempTable
create table M_GRADES (
  grade_id UNIQUEIDENTIFIER not null
  , grade_name NVARCHAR(20) not null
  , order_no INT not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

alter table M_GRADES add constraint M_GRADES_IX1
  unique (grade_name) ;

create unique index M_GRADES_PKI
  on M_GRADES(grade_id);

alter table M_GRADES
  add constraint M_GRADES_PKC primary key (grade_id);

-- フレーズ画像
--* RestoreFromTempTable
create table D_PHRASE_IMAGES (
  phrase_image_id UNIQUEIDENTIFIER not null
  , url NVARCHAR(500) not null
  , upload_at DATETIME
  , phrase_id UNIQUEIDENTIFIER not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

alter table D_PHRASE_IMAGES add constraint D_PHRASE_IMAGES_IX1
  unique (phrase_id) ;

create unique index D_PHRASE_IMAGES_PKI
  on D_PHRASE_IMAGES(phrase_image_id);

alter table D_PHRASE_IMAGES
  add constraint D_PHRASE_IMAGES_PKC primary key (phrase_image_id);

-- テスト結果明細
--* RestoreFromTempTable
create table D_TEST_RESULT_DETAILS (
  test_id UNIQUEIDENTIFIER not null
  , test_result_detail_no INT not null
  , phrase_id UNIQUEIDENTIFIER not null
  , is_correct BIT default 0 not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_TEST_RESULT_DETAILS_IX1
  on D_TEST_RESULT_DETAILS(phrase_id);

create unique index D_TEST_RESULT_DETAILS_PKI
  on D_TEST_RESULT_DETAILS(test_id,test_result_detail_no);

alter table D_TEST_RESULT_DETAILS
  add constraint D_TEST_RESULT_DETAILS_PKC primary key (test_id,test_result_detail_no);

-- テスト結果
--* RestoreFromTempTable
create table D_TEST_RESULTS (
  test_id UNIQUEIDENTIFIER not null
  , test_datetime DATETIME not null
  , grade_id UNIQUEIDENTIFIER not null
  , complete_flg BIT not null
  , user_id NVARCHAR(450)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_TEST_RESULTS_IX1
  on D_TEST_RESULTS(user_id,test_datetime);

create index D_TEST_RESULTS_IX2
  on D_TEST_RESULTS(grade_id);

create unique index D_TEST_RESULTS_PKI
  on D_TEST_RESULTS(test_id);

alter table D_TEST_RESULTS
  add constraint D_TEST_RESULTS_PKC primary key (test_id);

-- フレーズジャンル
--* RestoreFromTempTable
create table M_PHRASE_GENRES (
  phrase_id UNIQUEIDENTIFIER not null
  , sub_genre_id UNIQUEIDENTIFIER not null
  , genre_id UNIQUEIDENTIFIER not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index M_PHRASE_GENRES_IX1
  on M_PHRASE_GENRES(phrase_id);

create index M_PHRASE_GENRES_IX2
  on M_PHRASE_GENRES(genre_id,sub_genre_id);

create unique index M_PHRASE_GENRES_PKI
  on M_PHRASE_GENRES(phrase_id,sub_genre_id,genre_id);

alter table M_PHRASE_GENRES
  add constraint M_PHRASE_GENRES_PKC primary key (phrase_id,sub_genre_id,genre_id);

-- サブジャンルマスタ
--* RestoreFromTempTable
create table M_SUB_GENRES (
  genre_id UNIQUEIDENTIFIER not null
  , sub_genre_id UNIQUEIDENTIFIER not null
  , sub_genre_name NVARCHAR(30) not null
  , order_no INT not null
  , is_default BIT default 0 not null
  , user_id NVARCHAR(450)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index M_SUB_GENRES_IX1
  on M_SUB_GENRES(user_id,genre_id,sub_genre_name);

create unique index M_SUB_GENRES_PKI
  on M_SUB_GENRES(genre_id,sub_genre_id);

alter table M_SUB_GENRES
  add constraint M_SUB_GENRES_PKC primary key (genre_id,sub_genre_id);

-- ジャンルマスタ
--* RestoreFromTempTable
create table M_GENRES (
  genre_id UNIQUEIDENTIFIER not null
  , genre_name NVARCHAR(30) not null
  , user_id nvARCHAR(450)
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index M_GENRES_IX1
  on M_GENRES(user_id,genre_name);

create unique index M_GENRES_PKI
  on M_GENRES(genre_id);

alter table M_GENRES
  add constraint M_GENRES_PKC primary key (genre_id);

-- ユーザーフレーズ
--* RestoreFromTempTable
create table D_PHRASES (
  phrase_id UNIQUEIDENTIFIER not null
  , phrase VARCHAR(200) not null
  , meaning NVARCHAR(200)
  , note NVARCHAR(200)
  , user_id NVARCHAR(450) not null
  , created_at DATETIME default GETDATE() not null
  , updated_at DATETIME default GETDATE() not null
) ;

create index D_PHRASES_IX1
  on D_PHRASES(user_id);

create index D_PHRASES_IX2
  on D_PHRASES(user_id,created_at desc);

create unique index D_PHRASES_PKI
  on D_PHRASES(phrase_id);

alter table D_PHRASES
  add constraint D_PHRASES_PKC primary key (phrase_id);

alter table M_PHRASE_BOOK_ITEMS
  add constraint M_PHRASE_BOOK_ITEMS_FK1 foreign key (phrase_book_id) references M_PHRASE_BOOKS(phrase_book_id)
  on delete no action;

alter table M_PHRASE_BOOK_ITEMS
  add constraint M_PHRASE_BOOK_ITEMS_FK2 foreign key (phrase_id) references D_PHRASES(phrase_id)
  on delete no action;

alter table D_DAILY_USAGE
  add constraint D_DAILY_USAGE_FK1 foreign key (operation_type_id) references M_OPERATION_TYPES(operation_type_id)
  on delete no action;

alter table D_ENGLISH_DIARY_TAGS
  add constraint D_ENGLISH_DIARY_TAGS_FK1 foreign key (diary_tag_id) references M_DIARY_TAGS(tag_id)
  on delete no action;

alter table D_ENGLISH_DIARY_TAGS
  add constraint D_ENGLISH_DIARY_TAGS_FK2 foreign key (diary_id) references D_ENGLISH_DIARYS(diary_id)
  on delete no action;

alter table D_TEST_RESULT_DETAILS
  add constraint D_TEST_RESULT_DETAILS_FK1 foreign key (phrase_id) references D_PHRASES(phrase_id)
  on delete no action;

alter table D_PHRASE_IMAGES
  add constraint D_PHRASE_IMAGES_FK1 foreign key (phrase_id) references D_PHRASES(phrase_id)
  on delete no action;

alter table D_REVIEW_LOGS
  add constraint D_REVIEW_LOGS_FK1 foreign key (phrase_id) references D_PHRASES(phrase_id)
  on delete no action;

alter table M_PHRASE_GENRES
  add constraint M_PHRASE_GENRES_FK1 foreign key (phrase_id) references D_PHRASES(phrase_id)
  on delete no action;

alter table M_PHRASE_GENRES
  add constraint M_PHRASE_GENRES_FK2 foreign key (genre_id,sub_genre_id) references M_SUB_GENRES(genre_id,sub_genre_id)
  on delete no action;

alter table D_REVIEW_LOGS
  add constraint D_REVIEW_LOGS_FK2 foreign key (test_id,test_result_detail_no) references D_TEST_RESULT_DETAILS(test_id,test_result_detail_no)
  on delete no action;

alter table D_REVIEW_LOGS
  add constraint D_REVIEW_LOGS_FK3 foreign key (review_type_id) references M_REVIEW_TYPES(review_type_id)
  on delete no action;

alter table D_TEST_RESULTS
  add constraint D_TEST_RESULTS_FK1 foreign key (grade_id) references M_GRADES(grade_id)
  on delete no action;

alter table D_TEST_RESULT_DETAILS
  add constraint D_TEST_RESULT_DETAILS_FK2 foreign key (test_id) references D_TEST_RESULTS(test_id)
  on delete no action;

alter table M_SUB_GENRES
  add constraint M_SUB_GENRES_FK1 foreign key (genre_id) references M_GENRES(genre_id)
  on delete no action;

execute sp_addextendedproperty N'MS_Description', N'フレーズ帳アイテム', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', null, null;
execute sp_addextendedproperty N'MS_Description', N'フレーズ帳Id', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'phrase_book_id';
execute sp_addextendedproperty N'MS_Description', N'フレーズId', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'ソート順', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'order_no';
execute sp_addextendedproperty N'MS_Description', N'メモ', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'note';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOK_ITEMS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'フレーズ帳マスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', null, null;
execute sp_addextendedproperty N'MS_Description', N'フレーズ帳Id', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'phrase_book_id';
execute sp_addextendedproperty N'MS_Description', N'フレーズ帳名', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'phrase_book_name';
execute sp_addextendedproperty N'MS_Description', N'説明', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'description';
execute sp_addextendedproperty N'MS_Description', N'ユーザーId', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_BOOKS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'操作種別マスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', null, null;
execute sp_addextendedproperty N'MS_Description', N'操作種別ID', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'operation_type_id';
execute sp_addextendedproperty N'MS_Description', N'操作種別コード', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'operation_type_code';
execute sp_addextendedproperty N'MS_Description', N'操作種別名', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'operation_type_name';
execute sp_addextendedproperty N'MS_Description', N'操作回数上限', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'operation_type_limit';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_OPERATION_TYPES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'操作履歴', N'SCHEMA', N'dbo', N'TABLE', N'D_DAILY_USAGE', null, null;
execute sp_addextendedproperty N'MS_Description', N'ユーザーID', N'SCHEMA', N'dbo', N'TABLE', N'D_DAILY_USAGE', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'操作日', N'SCHEMA', N'dbo', N'TABLE', N'D_DAILY_USAGE', N'COLUMN', N'operation_date';
execute sp_addextendedproperty N'MS_Description', N'操作種別ID', N'SCHEMA', N'dbo', N'TABLE', N'D_DAILY_USAGE', N'COLUMN', N'operation_type_id';
execute sp_addextendedproperty N'MS_Description', N'操作回数', N'SCHEMA', N'dbo', N'TABLE', N'D_DAILY_USAGE', N'COLUMN', N'operation_count';

execute sp_addextendedproperty N'MS_Description', N'格言マスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', null, null;
execute sp_addextendedproperty N'MS_Description', N'格言ID', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'proverb_id';
execute sp_addextendedproperty N'MS_Description', N'格言', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'proverb_text';
execute sp_addextendedproperty N'MS_Description', N'意味', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'meaning';
execute sp_addextendedproperty N'MS_Description', N'著者', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'author';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PROVERBS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'英語日記タグ', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARY_TAGS', null, null;
execute sp_addextendedproperty N'MS_Description', N'英語日記ID', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARY_TAGS', N'COLUMN', N'diary_id';
execute sp_addextendedproperty N'MS_Description', N'日記タグID', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARY_TAGS', N'COLUMN', N'diary_tag_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARY_TAGS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARY_TAGS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'日記タグ', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', null, null;
execute sp_addextendedproperty N'MS_Description', N'日記タグID', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', N'COLUMN', N'tag_id';
execute sp_addextendedproperty N'MS_Description', N'タグ名', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', N'COLUMN', N'tag_name';
execute sp_addextendedproperty N'MS_Description', N'ユーザーID', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_DIARY_TAGS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'英語日記', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', null, null;
execute sp_addextendedproperty N'MS_Description', N'英語日記ID', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'diary_id';
execute sp_addextendedproperty N'MS_Description', N'タイトル', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'title';
execute sp_addextendedproperty N'MS_Description', N'内容', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'content';
execute sp_addextendedproperty N'MS_Description', N'補足', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'note';
execute sp_addextendedproperty N'MS_Description', N'添削結果', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'correction';
execute sp_addextendedproperty N'MS_Description', N'解説', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'explanation';
execute sp_addextendedproperty N'MS_Description', N'ユーザーId', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_ENGLISH_DIARYS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'復習種別マスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_REVIEW_TYPES', null, null;
execute sp_addextendedproperty N'MS_Description', N'復習種別ID', N'SCHEMA', N'dbo', N'TABLE', N'M_REVIEW_TYPES', N'COLUMN', N'review_type_id';
execute sp_addextendedproperty N'MS_Description', N'復習種別名', N'SCHEMA', N'dbo', N'TABLE', N'M_REVIEW_TYPES', N'COLUMN', N'review_type_name';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_REVIEW_TYPES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_REVIEW_TYPES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'復習履歴', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', null, null;
execute sp_addextendedproperty N'MS_Description', N'復習履歴ID', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'review_id ';
execute sp_addextendedproperty N'MS_Description', N'フレーズID', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'復習日', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'review_date';
execute sp_addextendedproperty N'MS_Description', N'復習種別ID', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'review_type_id';
execute sp_addextendedproperty N'MS_Description', N'テスト結果ID', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'test_id';
execute sp_addextendedproperty N'MS_Description', N'テスト結果明細連番', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'test_result_detail_no';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_REVIEW_LOGS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'成績マスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', null, null;
execute sp_addextendedproperty N'MS_Description', N'成績ID', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', N'COLUMN', N'grade_id';
execute sp_addextendedproperty N'MS_Description', N'成績名', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', N'COLUMN', N'grade_name';
execute sp_addextendedproperty N'MS_Description', N'ソート順', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', N'COLUMN', N'order_no';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_GRADES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'フレーズ画像', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', null, null;
execute sp_addextendedproperty N'MS_Description', N'フレーズ画像ID', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'phrase_image_id';
execute sp_addextendedproperty N'MS_Description', N'URL', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'url';
execute sp_addextendedproperty N'MS_Description', N'アップロード日時', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'upload_at';
execute sp_addextendedproperty N'MS_Description', N'フレーズID', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASE_IMAGES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'テスト結果明細', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', null, null;
execute sp_addextendedproperty N'MS_Description', N'テスト結果ID', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'test_id';
execute sp_addextendedproperty N'MS_Description', N'テスト結果明細連番', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'test_result_detail_no';
execute sp_addextendedproperty N'MS_Description', N'フレーズID', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'正解フラグ', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'is_correct';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULT_DETAILS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'テスト結果', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', null, null;
execute sp_addextendedproperty N'MS_Description', N'テスト結果ID', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'test_id';
execute sp_addextendedproperty N'MS_Description', N'テスト日時', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'test_datetime';
execute sp_addextendedproperty N'MS_Description', N'成績ID', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'grade_id';
execute sp_addextendedproperty N'MS_Description', N'完了フラグ', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'complete_flg';
execute sp_addextendedproperty N'MS_Description', N'ユーザーID', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_TEST_RESULTS', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'フレーズジャンル', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', null, null;
execute sp_addextendedproperty N'MS_Description', N'フレーズID', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'サブジャンルID', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', N'COLUMN', N'sub_genre_id';
execute sp_addextendedproperty N'MS_Description', N'ジャンルID', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', N'COLUMN', N'genre_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_PHRASE_GENRES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'サブジャンルマスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', null, null;
execute sp_addextendedproperty N'MS_Description', N'ジャンルID', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'genre_id';
execute sp_addextendedproperty N'MS_Description', N'サブジャンルID', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'sub_genre_id';
execute sp_addextendedproperty N'MS_Description', N'サブジャンル名', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'sub_genre_name';
execute sp_addextendedproperty N'MS_Description', N'ソート順', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'order_no';
execute sp_addextendedproperty N'MS_Description', N'デフォルトフラグ', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'is_default';
execute sp_addextendedproperty N'MS_Description', N'ユーザーId', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_SUB_GENRES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'ジャンルマスタ', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', null, null;
execute sp_addextendedproperty N'MS_Description', N'ジャンルID', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', N'COLUMN', N'genre_id';
execute sp_addextendedproperty N'MS_Description', N'ジャンル名', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', N'COLUMN', N'genre_name';
execute sp_addextendedproperty N'MS_Description', N'ユーザーId', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'M_GENRES', N'COLUMN', N'updated_at';

execute sp_addextendedproperty N'MS_Description', N'ユーザーフレーズ', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', null, null;
execute sp_addextendedproperty N'MS_Description', N'フレーズID', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'phrase_id';
execute sp_addextendedproperty N'MS_Description', N'フレーズ', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'phrase';
execute sp_addextendedproperty N'MS_Description', N'意味', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'meaning';
execute sp_addextendedproperty N'MS_Description', N'備考', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'note';
execute sp_addextendedproperty N'MS_Description', N'ユーザーID', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'user_id';
execute sp_addextendedproperty N'MS_Description', N'作成日時', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'created_at';
execute sp_addextendedproperty N'MS_Description', N'更新日時', N'SCHEMA', N'dbo', N'TABLE', N'D_PHRASES', N'COLUMN', N'updated_at';

CREATE UNIQUE INDEX M_SUB_GENRES_IX_DEFAULT
ON M_SUB_GENRES(user_id, genre_id)
WHERE [is_default] = 1;
