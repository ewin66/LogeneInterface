﻿using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LGInterface.Util;
using SendPisResult;
using zlPacsInterface;

namespace LGInterface
{
    public class LGInterface
    {
        IniFiles f = new IniFiles("sz.ini");
        private string ConfigSection = "广西大化县人民医院";
        private clsPacsInterface zlInterface;


        public LGInterface()
        {
        }

        private void IniZlInterface()
        {
            int lngDepartmentId = -1;
            string strServerName = "";
            string strUserName = "";
            string strUserPwd = "";
            string strNullValue = "";
            int SysNo = -1;
            string SysOwner = "";
            string SplitChar = "|";

            //初始化
            zlInterface = new clsPacsInterfaceClass();

            var iniSuccess = zlInterface.InitInterface(lngDepartmentId, strServerName, strUserName, strUserPwd, SysNo,
                SysOwner,
                strNullValue, SplitChar, TErrorShowType.estShowMsg);
            if (iniSuccess == false)
            {
                log.WriteMyLog("中联接口初始化失败!");
                throw new Exception("中联接口初始化失败");
            }
        }

        //exid 放到申请序号
        //处方序号 放到就诊ID
        //CARDNUMBER 这个放到身份证号
        //string sHISName, string Sslbx"门诊号","住院号" , string Ssbz "标记", string Debug, string by
        //该接口用发票号代替门诊号,不存门诊号
        public string LGGetHISINFO(string sHISName, string Sslbx, string Ssbz, string Debug, string by)
        {
            //log.开始请求申请信息,入参sslbx=,ssbz=

            log.WriteMyLog($"开始调用接口请求患者信息:sslbx={Sslbx},ssbz={Ssbz}.");

            //ssbz=HIS医嘱ID,该号码必须是数字,否则接口不做处理
            var intSsbz = 0;
            try
            {
                intSsbz = Convert.ToInt32(Ssbz.Trim());
            }
            catch (Exception)
            {
                log.WriteMyLog("HIS医嘱序号不是纯数字,操作取消.");
                return "0";
            }

            try
            {
                IniZlInterface();
            }
            catch (Exception e)
            {
                MessageBox.Show("中联接口初始化失败");
                return "0";
            }
            
            if (Sslbx != "删除病例")
                return GetHisReq(Sslbx, Ssbz);
            else
                return CancelHisReq(Ssbz);
        }


        private string GetHisReq(string Sslbx, string Ssbz)
        {
            //查询类别,:rwtPatientId = 1   '病人ID
            //  rwtInHospital = 2  '住院号
            //  rwtOutPatient = 3  '门诊号
            //  rwtSickCard = 4    '就诊卡号
            //  rwtIdCard = 5      '身份证号
            //  rwtHealthNum = 6   '体检号
            //  rwtPatientName = 7 '病人姓名
            //  rwtAdviceId = 8    '医嘱ID

            //取信息逻辑:
            //1.提取申请单信息
            //2.如果没取到,可能是因为已接收,尝试取消接收医嘱后,再次提取
            //3.还提取不到,返回"0"
            LgXml lgXml = null;
            bool fristSuccess = false;
            try
            {
                //第一次提取
                lgXml = GetHisReqXml(Ssbz, Sslbx);
                fristSuccess = true;
            }
            catch (Exception e)
            {
                log.WriteMyLog("首次提取HIS申请单失败:"+e.Message);
                fristSuccess = false;
            }
            //第一次没成功,开始第二次提取
            if (!fristSuccess)
            {
                log.WriteMyLog("第一次提取失败,尝试先取消签收后再次提取");
                //先取消接收
                CancelHisReq(Ssbz);
                try
                {
                    //第二次提取
                    lgXml = GetHisReqXml(Ssbz, Sslbx);
                }
                catch (Exception e)
                {
                    log.WriteMyLog("第二次提取HIS申请单失败:" + e.Message);
                    MessageBox.Show("提取HIS申请单失败:" + e.Message);
                    return "0";
                }
            }


            //  GetDataSetByXml(sxml);
            string xml = "";
            try
            {
                xml = "<?xml version=" + (char) 34 + "1.0" + (char) 34 + " encoding=" + (char) 34 + "gbk" + (char) 34 +
                      "?>";
                xml = xml + "<LOGENE>";
                xml = xml + "<row ";
                xml = xml + "病人编号=" + (char) 34 + lgXml.病人编号 + (char) 34 + " ";
                xml = xml + "就诊ID=" + (char) 34 + lgXml.就诊ID + (char) 34 + " ";
                xml = xml + "申请序号=" + (char) 34 + lgXml.申请序号 + (char) 34 + " ";
                xml = xml + "门诊号=" + (char) 34 + lgXml.门诊号 + (char) 34 + " ";
                xml = xml + "住院号=" + (char) 34 + lgXml.住院号 + (char) 34 + " ";
                xml = xml + "姓名=" + (char) 34 + lgXml.姓名 + (char) 34 + " ";
                xml = xml + "性别=" + (char) 34 + lgXml.性别 + (char) 34 + " ";
                xml = xml + "年龄=" + (char) 34 + lgXml.年龄 + (char) 34 + " ";
                xml = xml + "婚姻=" + (char) 34 + lgXml.婚姻 + (char) 34 + " ";
                xml = xml + "地址=" + (char) 34 + lgXml.地址 + (char) 34 + " ";
                xml = xml + "电话=" + (char) 34 + lgXml.电话 + (char) 34 + " ";
                xml = xml + "病区=" + (char) 34 + lgXml.病区 + (char) 34 + " ";
                xml = xml + "床号=" + (char) 34 + lgXml.床号 + (char) 34 + " ";
                xml = xml + "身份证号=" + (char) 34 + lgXml.身份证号 + (char) 34 + " ";
                xml = xml + "民族= " + (char) 34 + lgXml.民族 + (char) 34 + " ";
                xml = xml + "职业=" + (char) 34 + lgXml.职业 + (char) 34 + " ";
                xml = xml + "送检科室=" + (char) 34 + lgXml.送检科室 + (char) 34 + " ";
                xml = xml + "送检医生=" + (char) 34 + lgXml.送检医生 + (char) 34 + " ";
                xml = xml + "收费=" + (char) 34 + lgXml.收费 + (char) 34 + " ";

                xml = xml + "标本名称=" + (char) 34 + lgXml.标本名称 + (char) 34 + " ";
                xml = xml + "送检医院=" + (char) 34 + lgXml.送检医院 + (char) 34 + " ";
                xml = xml + "医嘱项目=" + (char) 34 + lgXml.医嘱项目 + (char) 34 + " ";
                xml = xml + "备用1=" + (char) 34 + lgXml.备用1 + (char) 34 + " ";
                xml = xml + "备用2=" + (char) 34 + lgXml.备用2 + (char) 34 + " ";
                xml = xml + "费别=" + (char) 34 + lgXml.费别 + (char) 34 + " ";
                xml = xml + "病人类别=" + (char) 34 + lgXml.病人类别 + (char) 34 + " ";
                xml = xml + "/>";
                xml = xml + "<临床病史><![CDATA[" + lgXml.临床病史 + "]]></临床病史>";
                xml = xml + "<临床诊断><![CDATA[" + lgXml.临床诊断 + "]]></临床诊断>";
                xml = xml + "</LOGENE>";
            }
            catch (Exception e)
            {
                //log.解析返回的xml失败
                log.WriteMyLog("解析接口返回的xml时出错:" + e.Message);
                return "0";
            }

            //log.解析患者申请信息成功,返回xml
            log.WriteMyLog("调用接口请求患者信息成功!");

            //如果没找到病人,返回0,避免出错
            if (string.IsNullOrEmpty(xml.Trim()))
                xml = "0";

            return xml;
        }

        private string CancelHisReq(string ssbz)
        {
            var success = zlInterface.CancelRequest(Convert.ToInt32(ssbz), 0);
            if (!success)
            {
                log.WriteMyLog("HIS取消登记申请单失败:" + zlInterface.GetLastError());
                return "0";
            }
            return "1";
        }

        private LgXml GetHisReqXml(string Ssbz, string sslbx)
        {
            LgXml lgXml = new LgXml();

            TCusTable dtResult = new TCusTable();
            TCusTable dtPatient = new TCusTable();

            TRequestWhereType queryType = TRequestWhereType.rwtInHospital;

            var success = zlInterface.GetRequestInfo(Ssbz, queryType);
            if (!success)
            {
                var err = zlInterface.GetLastError();
                throw new Exception("中联接口提取失败,因为:" + err);
            }
            dtResult = zlInterface.Tables;
            if (dtResult.strColumns.Length == 0 || dtResult.strDatas.Length == 0)
            {
                throw new Exception("未找到HIS申请单信息");
            }

            success = zlInterface.GetPatientInfo(Ssbz, TPatientWhereType.pwtPatientId);
            if (!success)
            {
                var err = zlInterface.GetLastError();
                throw new Exception("中联接口提取失败,因为:" + err);
            }
            dtPatient = zlInterface.Tables;
            if (dtPatient.strColumns.Length == 0 || dtPatient.strDatas.Length == 0)
            {
                throw new Exception("未找到HIS病人信息");
            }

            lgXml.病人编号 = GetValue(dtResult, "病人id");
            lgXml.就诊ID = GetValue(dtResult, "门诊号");
            lgXml.申请序号 = GetValue(dtResult, "医嘱id");
            lgXml.门诊号 = GetValue(dtResult, "门诊号");
            lgXml.住院号 = GetValue(dtResult, "住院号");
            lgXml.姓名 = GetValue(dtResult, "姓名");
            lgXml.年龄 = GetValue(dtResult, "年龄");
            lgXml.性别 = GetValue(dtResult, "性别");
            lgXml.婚姻 = GetValue(dtPatient, "婚姻状况");
            lgXml.地址 = GetValue(dtPatient, "联系人地址");
            lgXml.电话 = GetValue(dtPatient, "联系人电话");
            // lgXml.病区 = GetValue(dtResult, "病人id");
            // lgXml.床号 = GetValue(dtResult, "病人id");
            lgXml.身份证号 = GetValue(dtPatient, "身份证号");
            // lgXml.民族 = GetValue(dtResult, "病人id");
            // lgXml.职业 = GetValue(dtResult, "病人id");
            lgXml.送检科室 = GetValue(dtResult, "申请科室");
            lgXml.送检医生 = GetValue(dtResult, "申请人");
            //lgXml.收费 = GetValue(dtResult, "病人id");
            //todo:标本名称可能要取 2.4.7.	GetAdviceItems
            //lgXml.标本名称 = GetValue(dtResult, "病人id");
            //lgXml.送检医院 = GetValue(dtResult, "病人id");
            lgXml.医嘱项目 = GetValue(dtResult, "医嘱内容");
            //lgXml.备用1 = GetValue(dtResult, "病人id");
            //lgXml.备用2 = GetValue(dtResult, "病人id");
            //lgXml.费别 = GetValue(dtResult, "病人id");
            lgXml.病人类别 = GetValue(dtResult, "病人来源");
            lgXml.临床病史 = GetValue(dtResult, "病历摘要");
            lgXml.临床诊断 = GetValue(dtResult, "临床诊断");

            return lgXml;
        }

        public static string GetValue(TCusTable dtResult, string colName)
        {
            int index = -1;
            for (int i = 0; i < dtResult.strColumns.Length; i++)
            {
                string colValue = dtResult.strColumns.GetValue(i).ToString();
                if (colValue == colName)
                {
                    index = i;
                    break;
                }
            }

            return dtResult.strDatas.GetValue(index).ToString();
        }
    }
}