﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasicInformation
{
    class Permissions
    {
        public static string 班級點名單_雙語部 { get { return "雙語部.BasicInformation.ClubPointForm"; } }
        public static bool 班級點名單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級點名單_雙語部].Executable;
            }
        }

        public static string 缺曠週報表_依假別_雙語部 { get { return "雙語部.BasicInformation.WARCByAbsence"; } }
        public static bool 缺曠週報表_依假別_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[缺曠週報表_依假別_雙語部].Executable;
            }
        }

        public static string 學生缺曠通知單_雙語部 { get { return "雙語部.BasicInformation.Notice_S"; } }
        public static bool 學生缺曠通知單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學生缺曠通知單_雙語部].Executable;
            }
        }

        public static string 班級缺曠通知單_雙語部 { get { return "雙語部.BasicInformation.Notice_C"; } }
        public static bool 班級缺曠通知單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級缺曠通知單_雙語部].Executable;
            }
        }

        public static string 學生獎懲通知單_雙語部 { get { return "雙語部.BasicInformation.Discipline_S"; } }
        public static bool 學生獎懲通知單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學生獎懲通知單_雙語部].Executable;
            }
        }

        public static string 班級獎懲通知單_雙語部 { get { return "雙語部.BasicInformation.Discipline_C"; } }
        public static bool 班級獎懲通知單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級獎懲通知單_雙語部].Executable;
            }
        }

        public static string 獎懲公告單_雙語部 { get { return "雙語部.BasicInformation.AnnouncementSingle.cs"; } }
        public static bool 獎懲公告單_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[獎懲公告單_雙語部].Executable;
            }
        }

        public static string 批次修改入學及畢業日期_雙語部 { get { return "雙語部.BasicInformation.SchoolYearEditor.cs"; } }
        public static bool 批次修改入學及畢業日期_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[批次修改入學及畢業日期_雙語部].Executable;
            }
        }

        public static string 匯出學生基本資料_雙語部 { get { return "雙語部.BasicInformation.ExportSchoolObject.cs"; } }
        public static bool 匯出學生基本資料_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出學生基本資料_雙語部].Executable;
            }
        }

        public static string 匯出學生基本資料New_雙語部 { get { return "雙語部.BasicInformation.ExportStudentData.cs"; } }
        public static bool 匯出學生基本資料New_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出學生基本資料New_雙語部].Executable;
            }
        }


        public static string 匯入學生基本資料_雙語部 { get { return "雙語部.BasicInformation.ImportSchoolObject.cs"; } }
        public static bool 匯入學生基本資料_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入學生基本資料_雙語部].Executable;
            }
        }

        public static string 匯入學生基本資料New_雙語部 { get { return "雙語部.BasicInformation.ImportStudentData.cs"; } }
        public static bool 匯入學生基本資料New_雙語部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入學生基本資料New_雙語部].Executable;
            }
        }

    }
}
