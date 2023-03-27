create table aichat.__efmigrationshistory
(
    MigrationId    varchar(150) not null
        primary key,
    ProductVersion varchar(32)  not null
);

create table aichat.CodeGradeEntity
(
    id             bigint auto_increment
        primary key,
    usageTime      int not null,
    usageFrequency int not null
);

create table aichat.ActivationCode
(
    id             bigint auto_increment
        primary key,
    code           longtext not null,
    CodeGradeID    bigint   not null,
    remainingTimes int      not null,
    constraint FK_ActivationCode_CodeGradeEntity_CodeGradeID
        foreign key (CodeGradeID) references aichat.CodeGradeEntity (id)
            on delete cascade
);

create index IX_ActivationCode_CodeGradeID
    on aichat.ActivationCode (CodeGradeID);

create table aichat.HistoryMessages
(
    id      bigint auto_increment
        primary key,
    role    longtext not null,
    content longtext not null
);

create table aichat.HistoryRecord
(
    id       bigint auto_increment
        primary key,
    messages longtext not null,
    uid      longtext not null
);

create table aichat.User
(
    uid            varchar(50) not null
        primary key,
    userName       longtext     not null,
    Password       longtext     not null,
    email          longtext     not null,
    grade          int          not null,
    expireDate     datetime(6)  not null,
    remainingTimes int          not null
);


