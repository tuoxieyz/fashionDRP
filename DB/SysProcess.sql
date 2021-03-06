USE [SysProcess]
GO
/****** Object:  Table [dbo].[SysUserRole]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysUserRole](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysUserRole] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysUserModule]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysUserModule](
	[UserId] [int] NOT NULL,
	[ModuleId] [int] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysUserModule] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[ModuleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysUserBrand_deleted]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysUserBrand_deleted](
	[UserID] [int] NOT NULL,
	[BrandID] [int] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysUserBrand] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC,
	[BrandID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysUser]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysUser](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Password] [varchar](50) NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[Flag] [bit] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_SysUser] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SysUser] ON [dbo].[SysUser] 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SysUser', @level2type=N'COLUMN',@level2name=N'Flag'
GO
/****** Object:  Table [dbo].[SysRoleModule]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysRoleModule](
	[RoleId] [int] NOT NULL,
	[ModuleId] [int] NOT NULL,
 CONSTRAINT [PK_SysRoleModule] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC,
	[ModuleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysRole]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysRole](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[Description] [varchar](200) NOT NULL,
	[ProductOP] [bit] NOT NULL,
	[OrganizationOP] [bit] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysRole] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_SysRole] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[OrganizationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'成品资料权限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SysRole', @level2type=N'COLUMN',@level2name=N'ProductOP'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'组织机构权限' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SysRole', @level2type=N'COLUMN',@level2name=N'OrganizationOP'
GO
/****** Object:  Table [dbo].[SysProvience]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysProvience](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProvienceName] [nvarchar](50) NOT NULL,
	[AreaId] [int] NOT NULL,
 CONSTRAINT [PK_SysProvience] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysOrganizationType]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysOrganizationType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[OrganizationID] [int] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysOrganizationType_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_SysOrganizationType] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[OrganizationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SysOrganizationBrand_deleted]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysOrganizationBrand_deleted](
	[OrganizationID] [int] NOT NULL,
	[BrandID] [int] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_SysOrganizationBrand] PRIMARY KEY CLUSTERED 
(
	[OrganizationID] ASC,
	[BrandID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysOrganization]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysOrganization](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[ParentID] [int] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[TypeId] [int] NOT NULL,
	[AreaID] [int] NOT NULL,
	[ProvienceID] [int] NOT NULL,
	[CityID] [int] NOT NULL,
	[Address] [nvarchar](100) NULL,
	[Telephone] [varchar](15) NULL,
	[Linkman] [varchar](50) NULL,
	[Flag] [bit] NOT NULL,
	[CreatorID] [int] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[Description] [varchar](200) NULL,
 CONSTRAINT [PK_SysOrganization] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SysOrganization', @level2type=N'COLUMN',@level2name=N'TypeId'
GO
/****** Object:  Table [dbo].[SysModule]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysModule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[ApplicationId] [int] NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[ParentCode] [varchar](50) NOT NULL,
	[Uri] [varchar](100) NOT NULL,
 CONSTRAINT [PK_SysModule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SysModule] ON [dbo].[SysModule] 
(
	[ApplicationId] ASC,
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysCity]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysCity](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CityName] [nvarchar](50) NOT NULL,
	[ProvienceId] [int] NOT NULL,
 CONSTRAINT [PK_SysCity] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SysArea]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysArea](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AreaName] [varchar](20) NOT NULL,
 CONSTRAINT [PK_SysArea] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SysApplication]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SysApplication](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](500) NULL,
 CONSTRAINT [PK_SysApplication] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SoftVersionTrack]    Script Date: 07/14/2012 10:08:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SoftVersionTrack](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SoftName] [varchar](50) NOT NULL,
	[VersionCode] [varchar](50) NOT NULL,
	[VersionServiceUrl] [varchar](50) NOT NULL,
	[Description] [varchar](max) NULL,
	[UpdatedFileList] [xml] NOT NULL,
	[IsCoerciveUpdate] [bit] NOT NULL,
 CONSTRAINT [PK_SoftVersionTrack] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SoftVersionTrack] ON [dbo].[SoftVersionTrack] 
(
	[SoftName] ASC,
	[VersionCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[GetRolesUserCover]    Script Date: 07/14/2012 10:08:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		徐时进
-- Create date: 2012-04-06
-- Description:	用户所能支配的角色集合(角色所对应的权限当前用户都拥有，且是用户所在机构的角色)
-- exec GetRolesUserCover 1
-- =============================================
CREATE PROCEDURE [dbo].[GetRolesUserCover]
	@UserID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--用户拥有的所有权限
	select rmi.ModuleId into #mi from SysUserRole ur 
	inner join SysRoleModule rmi 
	on ur.RoleId = rmi.RoleId and ur.UserId=@UserID
	
	--用户所在机构的所有角色
	select distinct r.* into #ri from SysRole r
	inner join SysUser u
	on r.OrganizationID = u.OrganizationID and u.ID=@UserID
	
	select * from #ri where ID not in
	(
		select distinct r.ID from #ri r
		inner join SysRoleModule rm
		on r.ID = rm.RoleId and not exists(select 1 from #mi where rm.ModuleId=#mi.ModuleId)
	)
	
END
GO
/****** Object:  StoredProcedure [dbo].[GetFilesNeedUpdate]    Script Date: 07/14/2012 10:08:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		徐时进
-- Create date: 2012-06-07
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetFilesNeedUpdate]
	@SoftName varchar(50),
	@NowVersion varchar(50)
AS
BEGIN
	
	SET NOCOUNT ON;

	SELECT [VersionCode]
      ,[VersionServiceUrl]
      ,[Description]
      ,[UpdatedFileList] 
      ,IsCoerciveUpdate
      from SoftVersionTrack 
      where SoftName=@SoftName and VersionCode>@NowVersion
      order by [VersionCode] desc
END
GO
/****** Object:  Default [DF_SysUserRole_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUserRole] ADD  CONSTRAINT [DF_SysUserRole_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysUserModule_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUserModule] ADD  CONSTRAINT [DF_SysUserModule_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysUserBrand_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUserBrand_deleted] ADD  CONSTRAINT [DF_SysUserBrand_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysUser_Password]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUser] ADD  CONSTRAINT [DF_SysUser_Password]  DEFAULT ((123)) FOR [Password]
GO
/****** Object:  Default [DF_SysUser_Status]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUser] ADD  CONSTRAINT [DF_SysUser_Status]  DEFAULT ((1)) FOR [Flag]
GO
/****** Object:  Default [DF_SysUser_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysUser] ADD  CONSTRAINT [DF_SysUser_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysRole_ProductOP]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysRole] ADD  CONSTRAINT [DF_SysRole_ProductOP]  DEFAULT ((0)) FOR [ProductOP]
GO
/****** Object:  Default [DF_SysRole_OrganizationOP]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysRole] ADD  CONSTRAINT [DF_SysRole_OrganizationOP]  DEFAULT ((0)) FOR [OrganizationOP]
GO
/****** Object:  Default [DF_SysRole_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysRole] ADD  CONSTRAINT [DF_SysRole_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysOrganizationType_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysOrganizationType] ADD  CONSTRAINT [DF_SysOrganizationType_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysOrganizationBrand_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysOrganizationBrand_deleted] ADD  CONSTRAINT [DF_SysOrganizationBrand_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SysOrganization_Flag]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysOrganization] ADD  CONSTRAINT [DF_SysOrganization_Flag]  DEFAULT ((1)) FOR [Flag]
GO
/****** Object:  Default [DF_SysOrganization_CreateTime]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SysOrganization] ADD  CONSTRAINT [DF_SysOrganization_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO
/****** Object:  Default [DF_SoftVersionTrack_IsCoerciveUpdate]    Script Date: 07/14/2012 10:08:01 ******/
ALTER TABLE [dbo].[SoftVersionTrack] ADD  CONSTRAINT [DF_SoftVersionTrack_IsCoerciveUpdate]  DEFAULT ((1)) FOR [IsCoerciveUpdate]
GO
