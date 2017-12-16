using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.IO;
using System.Data;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataSourcesFile;

namespace Geodatabase
{
    /// <summary>
    ///
    /// 功能描述:   静态类，处理和Geodatabase 相关的所有操作
    ///             包括：连接sde，创建workspace，创建featureclass、table，返回字段，查询、更新等等
    /// 开发者:     XXX
    /// 建立时间:   2008-10-8 0:00:00
    /// 修订描述:
    /// 进度描述:
    /// 版本号      :    1.0
    /// 最后修改时间:    2008-10-7 13:36:48
    ///
    /// </summary>
    public partial class GeodatabaseOp
    {
        public static double P2 = 206264.806247;        // 180/Math.PI*3600
        public static double DefaultDOMIAN_EXM = 200;

        /// <summary>
        ///
        /// 功能描述:   获得指定字段的值，以文本形式返回
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iRow"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        public static string GetStrFormatValue(IRow iRow, int nIndex)
        {
            IFields iFlds = iRow.Fields;
            IField iFld = iFlds.get_Field(nIndex);
            if (iFld == null)
            {
                return "##########";
            }

            object obj = iRow.get_Value(nIndex);

            return obj.ToString();
        }

        /// <summary>
        /// 功能描述:   根据字段名获得指定字段的值，以文本形式返回
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        /// </summary>
        /// <param name="ipRow"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public static string GetStrFormatValue(IRow ipRow, string FieldName)
        {
            if (ipRow == null)
                return "";

            IFields ipFields = ipRow.Fields;
            int iFieldIndex = ipFields.FindField(FieldName);

            //返回值为－1说明没找到相应的字段
            if (iFieldIndex == -1)
                return "";

            return GetStrFormatValue(ipRow, iFieldIndex);
        }

        /// <summary>
        ///
        /// 功能描述:   检查字段的有效性
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFldsToTest">被检测的字段</param>
        /// <param name="iValidateWS">目标工作空间</param>
        /// <param name="iInWS">源工作空间</param>
        /// <param name="ErrMsg">错误信息</param>
        /// <param name="iFixedFlds">检测后的字段</param>
        /// <returns></returns>
        public static bool CheckFields(IFields iFldsToTest, IWorkspace iValidateWS,IWorkspace iInWS, ref string ErrMsg, ref IFields iFixedFlds)
        {
            IFieldChecker iFldChk = new FieldChecker();

            iFldChk.ValidateWorkspace = iValidateWS;
            if (iInWS != null)
                iFldChk.InputWorkspace = iInWS;

            IEnumFieldError iEnumFldErr;
            IFieldError iFldErr;

            iFldChk.Validate(iFldsToTest, out iEnumFldErr, out iFixedFlds);
            if (iEnumFldErr == null)
            {
                return true;
            }

            string Meg, FldName, ErrInfo = "";
            esriFieldNameErrorType ErrType;
            IField iErrFld;
            int index;

            iFldErr = iEnumFldErr.Next();
            while (iFldErr != null)
            {
                //获得错误字段
                index = iFldErr.FieldIndex;
                iErrFld = iFldsToTest.get_Field(index);
                FldName = iErrFld.Name;

                //获得错误信息
                ErrType = iFldErr.FieldError;
                switch (ErrType)
                {
                    case esriFieldNameErrorType.esriSQLReservedWord:
                        ErrInfo = "名称为SQL保留字";
                        break;
                    case esriFieldNameErrorType.esriDuplicatedFieldName:
                        ErrInfo = "字段名重复";
                        break;
                    case esriFieldNameErrorType.esriInvalidCharacter:
                        ErrInfo = "字段名中有无效的字符";
                        break;
                    case esriFieldNameErrorType.esriInvalidFieldNameLength:
                        ErrInfo = "字段名过长";
                        break;
                    default:
                        break;
                }

                Meg = String.Format("字段{0}定义不符：{1}\n", FldName, ErrInfo);
                ErrMsg += Meg;
                iFldErr = iEnumFldErr.Next();
            }

            return false;
        }

        /// <summary>
        ///
        /// 功能描述:   创建图层过程中，设置图层别名
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatClass"></param>
        /// <param name="sLayerAlias"></param>
        public static void SetSysFieldAlias(IFeatureClass iFeatClass, string sLayerAlias)
        {
            IClassSchemaEdit iClassSchemaEdit;

            iClassSchemaEdit = (IClassSchemaEdit)iFeatClass;
            iClassSchemaEdit.AlterAliasName(sLayerAlias);
        }

        /// <summary>
        ///
        /// 功能描述:   整个数据集一起删除，删除之前检查是否有锁，如果有锁，则不作任何删除
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="datasetName"></param>
        public static void DeleteFeatDataset(IFeatureWorkspace iFeatWS, string datasetName)
        {
            IFeatureDataset iCurFeatDataset;

            try
            {
                //检查数据集是否存在
                iCurFeatDataset = iFeatWS.OpenFeatureDataset(datasetName);
                if (iCurFeatDataset == null)
                {
                    return;
                }
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeatDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return;
            }

            //检查数据集是否有锁
            try
            {
                string sTabName = "";
                string sUser = "";

                if (CheckFeatureDatasetHasLock(iCurFeatDataset, ref sTabName, ref sUser))
                {
                    throw new ApplicationException("表[" + sTabName + "]该图库被用户[" + sUser + "]锁定，无法对其进行删除操作。");
                }
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeatDataset:" + ex.Message);
                //throw ex;
            }

            //删除数据集
            try
            {
                iCurFeatDataset.Delete();
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeatDataset:" + ex.Message);
                //throw ex;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   检查数据集是否有锁
        ///             一旦检测到数据集内有图层含有锁，就返回有锁信息
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatDataset">被检查的数据集</param>
        /// <param name="sTabName"></param>
        /// <param name="sUser"></param>
        /// <returns>是否有锁</returns>
        public static bool CheckFeatureDatasetHasLock(IFeatureDataset iFeatDataset, ref string sTabName, ref string sUser)
        {
            IFeatureClassContainer iFeatClassContainer;
            IFeatureClass iFeatClass;
            int numFeatClasses;
            bool hasLock;

            iFeatClassContainer = (IFeatureClassContainer)iFeatDataset;
            numFeatClasses = iFeatClassContainer.ClassCount;

            hasLock = false;
            for (int i = 0; i < numFeatClasses; i++)
            {
                iFeatClass = iFeatClassContainer.get_Class(i);
                if (CheckFeatureClassHasLock(iFeatClass, ref sTabName, ref sUser))
                {
                    hasLock = true;
                    break;
                }
            }

            return hasLock;
        }

        /// <summary>
        ///
        /// 功能描述:   检查数据集是否有表被锁
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatClass">被检查是否有表的图层</param>
        /// <param name="sTabName"></param>
        /// <param name="sUser"></param>
        /// <returns>是否有锁</returns>
        public static bool CheckFeatureClassHasLock(IFeatureClass iFeatClass, ref string sTabName, ref string sUser)
        {
            IEnumSchemaLockInfo iEnumSchemaLockInfo;
            ISchemaLockInfo iSchemaLockInfo;
            ISchemaLock iSchemaLock;

            iSchemaLock = (ISchemaLock)iFeatClass;
            if (iSchemaLock == null)
            {
                throw new ApplicationException("RE-0002:指针为空\nError in CheckHasLock");
            }

            //通过设置锁成功与否判断该图层是否被其他人使用
            iSchemaLock.GetCurrentSchemaLocks(out iEnumSchemaLockInfo);
            iSchemaLockInfo = iEnumSchemaLockInfo.Next();
            if (iSchemaLockInfo == null)
            {
                return false;
            }

            if (iSchemaLockInfo.SchemaLockType == esriSchemaLock.esriExclusiveSchemaLock)
            {
                sTabName = iSchemaLockInfo.TableName;
                sUser = iSchemaLockInfo.UserName;

                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// 功能描述:   检查是否有表被锁,支持ISchemaLock即可
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iSchemaLock"></param>
        /// <param name="sTabName"></param>
        /// <param name="sUser"></param>
        /// <returns></returns>
        public static bool CheckHasLock(ISchemaLock iSchemaLock,ref string sTabName, ref string sUser)
        {
            IEnumSchemaLockInfo iEnumSchemaLockInfo;
            ISchemaLockInfo iSchemaLockInfo;

            if (iSchemaLock == null)
            {
                // // LogHelper.LogHelper.("CheckHasLock:RE-0002:指针为空\nError in CheckHasLock");
                throw new ApplicationException("RE-0002:指针为空\nError in CheckHasLock");
            }

            //通过设置锁成功与否判断该图层是否被其他人使用
            iSchemaLock.GetCurrentSchemaLocks(out iEnumSchemaLockInfo);
            iSchemaLockInfo = iEnumSchemaLockInfo.Next();

            if (iSchemaLockInfo.SchemaLockType == esriSchemaLock.esriExclusiveSchemaLock)
            {
                sTabName = iSchemaLockInfo.TableName;
                sUser = iSchemaLockInfo.UserName;

                return true;
            }

            return false;
        }


        /// <summary>
        ///
        /// 功能描述:   根据条件语句删除FeatureClass的要素
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatClass"></param>
        /// <param name="sWhereClause"></param>
        public static void DeleteFeaturesInLayer(IFeatureClass iFeatClass, string sWhereClause)
        {
            IFeatureCursor iFeatCursor;

            ISchemaLock iSchemaLock = (ISchemaLock)iFeatClass;
            iSchemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

            IQueryFilter iQueryFilter = new QueryFilter();
            iQueryFilter.WhereClause = sWhereClause;
            try
            {
                iFeatCursor = iFeatClass.Update(iQueryFilter, false);
            }

            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeaturesInLayer:" + ex.Message);
                iSchemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                throw new ApplicationException(ex.Message);
            }

            IDataset iDataset = (IDataset)iFeatClass;
            IWorkspace iWorkSpace = iDataset.Workspace;
            ITransactions iTrans;

            iTrans = (ITransactions)iWorkSpace;
            try
            {
                iTrans.StartTransaction();
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeaturesInLayer:" + ex.Message);
                iTrans.AbortTransaction();
                //ipStepProgressor->Hide();
                iSchemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                throw new ApplicationException(ex.Message + "\n\n事务启动失败!");
            }

            IFeature iFeature = iFeatCursor.NextFeature();
            while (iFeature != null)
            {
                try
                {
                    iFeatCursor.DeleteFeature();
                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeaturesInLayer:" + ex.Message);
                    iTrans.AbortTransaction();

                    iSchemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                    throw new ApplicationException(ex.Message + "\n\n数据无法清除!");
                }
                iFeature = iFeatCursor.NextFeature();

            }
            iTrans.CommitTransaction();

            iSchemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
        }

        /// <summary>
        ///
        /// 功能描述:   通过经纬度计算平面坐标
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="MajorAxis">椭球参考主轴半径</param>
        /// <param name="MinorAxis">椭球参考短轴半径</param>
        /// <param name="Eer">椭球曲率</param>
        /// <param name="Err">椭球曲率</param>
        /// <param name="Xoffset"></param>
        /// <param name="Yoffset"></param>
        /// <param name="B">纬度</param>
        /// <param name="L">经度</param>
        /// <param name="x">横向坐标</param>
        /// <param name="y">纵向坐标</param>
        /// <param name="L0">中央经线</param>
        public static void CalculateBL2XY(double MajorAxis, double MinorAxis, double Eer, double Err, double Xoffset, double Yoffset, double B, double L, ref double x, ref double y, double L0)
        {
            double B2;
            double L2;
            double AA, BB, CC;
            double X, N;
            double l2;
            double t, g;
            double cosB, sinB;

            B2 = B * 3600;
            L2 = L * 3600;

            l2 = L2 - L0 * 3600.0;
            AA = 1.0 + Eer * 3.0 / 4.0 + 45.0 * Eer * Eer / 64.0;
            BB = 3.0 * Eer / 4.0 + 15.0 * Eer * Eer / 16.0;
            CC = 15.0 * Eer * Eer / 64.0;
            X = MajorAxis * (1.0 - Eer) * (AA * B2 / P2 - BB / 2.0 * Math.Sin(2.0 * B2 / P2) + CC / 4.0 * Math.Sin(4 * B2 / P2));
            N = MajorAxis / Math.Sqrt(1.0 - Eer * Math.Sin(B2 / P2) * Math.Sin(B2 / P2));

            t = Math.Tan(B2 / P2);
            g = Math.Sqrt(Err * Math.Cos(B2 / P2) * Math.Cos(B2 / P2));

            sinB = Math.Sin(B2 / P2);
            cosB = Math.Cos(B2 / P2);

            y = l2 / P2 * N * cosB * (1.0 + l2 * l2 * cosB * cosB / (6.0 * P2 * P2) * (1.0 - t * t + g * g) +
                Math.Pow(l2 * cosB / (P2), 4.0) / 120.0 * (5.0 - 18.0 * t * t + Math.Pow(t, 4.0) + 14.0 * g * g
                - 58.0 * g * g * t * t));
            x = 1.0 + Math.Pow(l2 * cosB / P2, 2.0) / 12.0 * (5.0 - t * t + 9.0 * g * g + 4.0 * Math.Pow(g, 4.0)) +
                Math.Pow(l2 * cosB / P2, 4.0) / 360.0 * (61.0 - 58.0 * t * t + Math.Pow(t, 4.0));

            x = x * l2 * l2 / (2.0 * P2 * P2) * N * cosB * sinB + X + Xoffset;
            y = y + Yoffset;  //东伪偏移
        }

        /// <summary>
        ///
        /// 功能描述:   通过经纬度计算平面坐标
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iSpatialRef"></param>
        /// <param name="BMin"></param>
        /// <param name="LMin"></param>
        /// <param name="BMax"></param>
        /// <param name="LMax"></param>
        /// <returns></returns>
        public static bool SetDomain(ISpatialReference iSpatialRef, double BMin, double LMin, double BMax, double LMax)
        {
            double majorAxis;
            double minorAxis;
            double dEer;
            double dErr;
            double offsetX;
            double offsetY;
            double dXmin;
            double dXmax;
            double dYmin;
            double dYmax;
            double dL0;

            //投影参考信息
            IProjectedCoordinateSystem iProjCoordSys = iSpatialRef as IProjectedCoordinateSystem;
            if (iProjCoordSys == null)
                return false;

            //获取中央经线
            dL0 = iProjCoordSys.get_CentralMeridian(true);

            //获取椭球参考地参数
            IGeographicCoordinateSystem iGeoCoordSys = iProjCoordSys.GeographicCoordinateSystem;
            if (iGeoCoordSys == null)
                return false;

            IDatum iDatum = iGeoCoordSys.Datum;
            if (iDatum == null)
                return false;

            ISpheroid iSpheroid = iDatum.Spheroid;
            if (iSpheroid == null)
                return false;

            majorAxis = iSpheroid.SemiMajorAxis;
            minorAxis = iSpheroid.SemiMinorAxis;
            dEer = (majorAxis * majorAxis - minorAxis * minorAxis) / majorAxis / majorAxis;
            dErr = (majorAxis * majorAxis - minorAxis * minorAxis) / minorAxis / minorAxis;
            offsetY = iProjCoordSys.FalseEasting;
            offsetX = iProjCoordSys.FalseNorthing;

            //梯形分幅下,有变形,因而使用这样的方法并不是最小的，故进行了修改
            //EngineCommonUtili.CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
            //BMin, LMin, ref dXmin, ref dYmin, dL0);
            //EngineCommonUtili.CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
            //BMax, LMax, ref dXmax, ref dYmax, dL0);

            double x1 = 0.0, x2 = 0.0, x3 = 0.0, x4 = 0.0;
            double y1 = 0.0, y2 = 0.0, y3 = 0.0, y4 = 0.0;

            //比较4个角点的坐标,求出范围
            CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
                BMin, LMin, ref x1, ref y1, dL0);
            CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
                BMax, LMin, ref x2, ref y2, dL0);
            CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
                BMin, LMax, ref x3, ref y3, dL0);
            CalculateBL2XY(majorAxis, minorAxis, dEer, dErr, offsetX, offsetY,
                BMax, LMax, ref x4, ref y4, dL0);

            dXmin = x1;
            dXmin = dXmin < x2 ? dXmin : x2;
            dXmin = dXmin < x3 ? dXmin : x3;
            dXmin = dXmin < x4 ? dXmin : x4;

            dXmax = x1;
            dXmax = dXmax > x2 ? dXmax : x2;
            dXmax = dXmax > x3 ? dXmax : x3;
            dXmax = dXmax > x4 ? dXmax : x4;

            dYmin = y1;
            dYmin = dYmin < y2 ? dYmin : y2;
            dYmin = dYmin < y3 ? dYmin : y3;
            dYmin = dYmin < y4 ? dYmin : y4;

            dYmax = y1;
            dYmax = dYmax > y2 ? dYmax : y2;
            dYmax = dYmax > y3 ? dYmax : y3;
            dYmax = dYmax > y4 ? dYmax : y4;
            iSpatialRef.SetDomain(dYmin - DefaultDOMIAN_EXM,dYmax + DefaultDOMIAN_EXM,
                                  dXmin - DefaultDOMIAN_EXM,dXmax + DefaultDOMIAN_EXM);
            return true;
        }

        /// <summary>
        ///
        /// 功能描述:   通过多边形四个角点坐标创建多边形(矩形)
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="llx"></param>
        /// <param name="lly"></param>
        /// <param name="lux"></param>
        /// <param name="luy"></param>
        /// <param name="rux"></param>
        /// <param name="ruy"></param>
        /// <param name="rlx"></param>
        /// <param name="rly"></param>
        /// <returns>多边形几何(矩形)</returns>
        public static IGeometry CreateEnvelope(double llx, double lly, double lux, double luy, double rux, double ruy, double rlx, double rly)
        {
            object missing = Type.Missing;
            IPoint iPoint;

            IPolygon iPolygon = new PolygonClass();
            IPointCollection iPointCollection = (IPointCollection)iPolygon;

            iPoint = new PointClass();
            iPoint.X = llx;
            iPoint.Y = lly;
            iPointCollection.AddPoint(iPoint, ref missing, ref missing);	//增加结点

            iPoint.X = lux;
            iPoint.Y = luy;
            iPointCollection.AddPoint(iPoint, ref missing, ref missing);	//增加结点

            iPoint.X = rux;
            iPoint.Y = ruy;
            iPointCollection.AddPoint(iPoint, ref missing, ref missing);	//增加结点

            iPoint.X = rlx;
            iPoint.Y = rly;
            iPointCollection.AddPoint(iPoint, ref missing, ref missing);	//增加结点

            iPolygon.Close();

            IGeometry iGeometry = (IGeometry)iPolygon;

            return iGeometry;
        }

        /// <summary>
        ///
        /// 功能描述:   转换度分秒，将以度表示的dValue转换成以度degree分minute秒second表示
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dValue"></param>
        /// <param name="degree"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public static void ConvertDegree2DMS(double dValue, ref int degree, ref int minute, ref int second)
        {
            degree = (int)dValue;
            minute = (int)((dValue - degree) * 60);
            second = (int)(((dValue - degree) * 60 - minute) * 60 + 0.5);
        }

        /// <summary>
        ///
        /// 功能描述:   转换度分秒，将以度表示的dValue转换成以度degree分minute秒second表示
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dValue"></param>
        /// <param name="degree"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public static void ConvertDegree2DMS(double dValue, ref int degree, ref int minute, ref double second)
        {
            degree = (int)dValue;
            minute = (int)((dValue - degree) * 60);
            second = ((dValue - degree) * 60 - minute) * 60;
        }

        /// <summary>
        ///
        /// 功能描述:   转换度分秒，将以度degree分minute秒second表示的转换成以以度表示的dValue
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dValue"></param>
        /// <param name="degree"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public static void ConvertDMS2Degree(ref double dValue, int degree, int minute, int second)
        {
            dValue = (double)(degree + minute / 60.0 + second / 3600.0);
        }



        /// <summary>
        ///
        /// 功能描述:   由空间参考文件名（不带扩展名）生成对应的空间参考对象
        ///             系统支持的空间参考文件放置于系统目录的Coordinate Systems文件夹下
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="strSRName">全路径</param>
        /// <returns>ISpatialReference空间参考对象</returns>
        public static ISpatialReference GetSRObject(string strSRName)
        {
            ISpatialReference iSR = null;

            //未知坐标系
            if (!strSRName.Equals("UnKnown"))   //相同
            {
                iSR = new UnknownCoordinateSystemClass();
            }

            //其他
            if (!File.Exists(strSRName))
            {
                iSR = new UnknownCoordinateSystemClass();
            }

            ISpatialReferenceFactory iSRFac = new SpatialReferenceEnvironment();

            iSR = iSRFac.CreateESRISpatialReferenceFromPRJFile(strSRName);

            return iSR;   //接受返回值需要作null判断！
        }

        /// <summary>
        ///
        /// 功能描述:   创建FeatureDataset
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="ipFeatWS"></param>
        /// <param name="iSpatialref"></param>
        /// <param name="DatasetName"></param>
        /// <returns></returns>
        public static IFeatureDataset CreateFeatDataset(IFeatureWorkspace ipFeatWS, ISpatialReference iSpatialref, string DatasetName)
        {
            IFeatureDataset retValue = null;
            if (ipFeatWS == null)
            {
                return null;
            }
            try
            {
                retValue = ipFeatWS.CreateFeatureDataset(DatasetName, iSpatialref);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.CreateFeatDataset:" + ex.Message);
                throw new ApplicationException(ex.Message + "创建图库失败");
            }
            return retValue;
        }

        /// <summary>
        ///
        /// 功能描述:   在指定FeatureDataset下创建FeatureLayer图层〔不包括注记层〕
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="ipDataset"></param>
        /// <param name="ClsName"></param>
        /// <param name="ipFields"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureDataset ipDataset, string ClsName, IFields ipFields)
        {
            if (ipDataset == null)
            {
                return null;
            }

            IFeatureClass retValue = null;
            UID CLSID;

            CLSID = new UID();
            CLSID.Value = "esricore.Feature";
            try
            {
                retValue = ipDataset.CreateFeatureClass(ClsName, ipFields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", "");
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.CreateFeatureClass:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n创建要素类[" + ClsName + "]失败");
                //return false;
            }

            return retValue;
        }

        /// <summary>
        ///
        /// 功能描述:   在FeatureWorkspace下创建FeatureLayer图层〔不包括注记层〕
        /// 开发者:     XXX
        /// 建立时间:   08-10-24 16:00:00
        /// </summary>
        /// <param name="ipFeatWorkspace"></param>
        /// <param name="ClsName"></param>
        /// <param name="ipFields"></param>
        /// <returns></returns>
        public static IFeatureClass CreateFeatureClass(IFeatureWorkspace ipFeatWorkspace, string ClsName, IFields ipFields)
        {
            if (ipFeatWorkspace == null)
            {
                return null;
            }

            IFeatureClass retValue = null;
            UID CLSID;

            CLSID = new UID();
            CLSID.Value = "esricore.Feature";
            try
            {
                retValue = ipFeatWorkspace.CreateFeatureClass(ClsName, ipFields, CLSID, null, esriFeatureType.esriFTSimple, "Shape", "");
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.CreateFeatureClass:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n创建要素类[" + ClsName + "]失败");
                //return false;
            }

            return retValue;
        }

        /// <summary>
        ///
        /// 功能描述:   创建Geodatabase模型中的注记要素类（独立的或非独立的）
        ///             该函数根据传入参数生成并返回一个注记要素类指针
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="strName">创建的要素类名</param>
        /// <param name="iFeatureDataSet">要素集</param>
        /// <param name="dRefScale">比例尺</param>
        /// <returns>IFeatureClassPtr 创建出的要素类对象的指针</returns>
        public static IFeatureClass CreatWSAnnoFeatClass(IFeatureWorkspace iFeatWS, string strName,
                                            IFeatureDataset iFeatureDataSet,double dRefScale)
        {
            IFeatureWorkspaceAnno featureWorkspaceAnno = (IFeatureWorkspaceAnno)iFeatWS;
            //set up the reference scale
            IGraphicsLayerScale graphicLayerScale = new GraphicsLayerScaleClass();

            graphicLayerScale.Units = esriUnits.esriFeet;
            graphicLayerScale.ReferenceScale = dRefScale;
            //set up symbol collection
            ISymbolCollection symbolCollection = new SymbolCollectionClass();

            #region "MakeText"
            IFormattedTextSymbol myTextSymbol = new TextSymbolClass();

            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = 150;
            rgbColor.Green = 0;
            rgbColor.Blue = 0;
            myTextSymbol.Color = (ESRI.ArcGIS.Display.IColor)rgbColor;
            //Set other properties for myTextSymbol
            myTextSymbol.Angle = 0;
            myTextSymbol.RightToLeft = false;
            myTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABaseline;
            myTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHAFull;
            myTextSymbol.CharacterSpacing = 200;
            myTextSymbol.Case = esriTextCase.esriTCNormal;
            #endregion

            symbolCollection.set_Symbol(0, (ESRI.ArcGIS.Display.ISymbol)myTextSymbol);
            //set up the annotation labeling properties including the expression
            IAnnotateLayerProperties annoProps = new LabelEngineLayerPropertiesClass();
            annoProps.FeatureLinked = true;
            annoProps.AddUnplacedToGraphicsContainer = false;
            annoProps.CreateUnplacedElements = true;
            annoProps.DisplayAnnotation = true;
            annoProps.UseOutput = true;

            ILabelEngineLayerProperties layerEngineLayerProps = (ILabelEngineLayerProperties)annoProps;
            IAnnotationExpressionEngine annoExpressionEngine = new AnnotationVBScriptEngineClass();
            layerEngineLayerProps.ExpressionParser = annoExpressionEngine;
            layerEngineLayerProps.Expression = "[DESCRIPTION]";
            layerEngineLayerProps.IsExpressionSimple = true;
            layerEngineLayerProps.Offset = 0;
            layerEngineLayerProps.SymbolID = 0;
            layerEngineLayerProps.Symbol = myTextSymbol;

            IAnnotateLayerTransformationProperties annoLayerTransProp = (IAnnotateLayerTransformationProperties)annoProps;
            annoLayerTransProp.ReferenceScale = graphicLayerScale.ReferenceScale;
            annoLayerTransProp.Units = graphicLayerScale.Units;
            annoLayerTransProp.ScaleRatio = 1;

            IAnnotateLayerPropertiesCollection annoPropsColl = new AnnotateLayerPropertiesCollectionClass();
            annoPropsColl.Add(annoProps);
            //use the AnnotationFeatureClassDescription to get the list of required
            //fields and the default name of the shape field
            IObjectClassDescription oCDesc = new AnnotationFeatureClassDescriptionClass();
            IFeatureClassDescription fCDesc = (IFeatureClassDescription)oCDesc;

            //create the new class
            return featureWorkspaceAnno.CreateAnnotationClass(strName,
                oCDesc.RequiredFields, oCDesc.InstanceCLSID, oCDesc.ClassExtensionCLSID,
                fCDesc.ShapeFieldName, "", iFeatureDataSet, null,
                annoPropsColl, graphicLayerScale, symbolCollection, true);
        }


        /// <summary>
        ///
        /// 功能描述:   创建个人Geodatabase数据库
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorkspace Create_pGDB_Workspace(string path, string pgdbname)
        {
            // Create an Access workspace factory.
            ESRI.ArcGIS.Geodatabase.IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();

            // Create an Access workspace and personal geodatabase.
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create(path, pgdbname, null, 0);

            // Cast for IName.
            IName name = workspaceName as IName;

            //Open a reference to the Access workspace through the name object.
            ESRI.ArcGIS.Geodatabase.IWorkspace pGDB_Wor = (IWorkspace)name.Open();

            return pGDB_Wor;
        }

        /// <summary>
        ///
        /// 功能描述:   创建文件Geodatabase数据库
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorkspace Create_fGDB_Workspace(string path, string fgdbname)
        {
            // Create a file geodatabase workspace factory.
            ESRI.ArcGIS.Geodatabase.IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();

            // Create a new file geodatabase.
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create(path, fgdbname, null, 0);

            // Cast for IName.
            ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)workspaceName;

            //Open a reference to the file geodatabase workspace through the name object.
            ESRI.ArcGIS.Geodatabase.IWorkspace fGDB_Wor = (IWorkspace)name.Open();

            return fGDB_Wor;
        }

        /// <summary>
        ///
        /// 功能描述:   创建连接SDE的属性文件〔ArcCatalog可以直接调用，主要Geoprossor需要使用〕
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="server"></param>
        /// <param name="instance"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool Create_ArcSDE_ConnectionFile(string path, string name, string server, string instance, string user, string password, string database, string version)
        {
            string fileName = System.IO.Path.Combine(path, name);
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("SERVER", server);
            propertySet.SetProperty("INSTANCE", instance);
            propertySet.SetProperty("DATABASE", database);
            propertySet.SetProperty("USER", user);
            propertySet.SetProperty("PASSWORD", password);
            propertySet.SetProperty("VERSION", version);

            ESRI.ArcGIS.Geodatabase.IWorkspaceFactory2 workspaceFactory;
            workspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory2)new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();
            workspaceFactory.Create(path, name, propertySet, 0);

            return true;
        }

        /// <summary>
        ///
        /// 功能描述:   创建工作组geodatabase，依赖于SQL Express
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="ServerName">"tivo\\sqlexpress"</param>
        /// <returns></returns>
        public static IWorkspace Create_fwGDB_Workspace(string ServerName, string database)
        {
            // Create a data server manager object.
            ESRI.ArcGIS.DataSourcesGDB.IDataServerManager dataserverManager = new ESRI.ArcGIS.DataSourcesGDB.DataServerManagerClass();

            // Set the server name and connect to the server.
            dataserverManager.ServerName = ServerName;
            dataserverManager.Connect();

            // Open one of the geodatabases in the database server.
            ESRI.ArcGIS.DataSourcesGDB.IDataServerManagerAdmin dataservermanagerAdmin = (ESRI.ArcGIS.DataSourcesGDB.IDataServerManagerAdmin)dataserverManager;
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = dataservermanagerAdmin.CreateWorkspaceName(database, "VERSION", "dbo.Default");
            ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)workspaceName;
            ESRI.ArcGIS.Geodatabase.IWorkspace GDB_wor = (IWorkspace)name.Open();

            return GDB_wor;
        }

        /// <summary>
        ///
        /// 功能描述:   创建shapefile工作空间
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorkspace Create_shapefile_Workspace(string path, string shapefilename)
        {
            ESRI.ArcGIS.Geodatabase.IWorkspaceFactory2 workspaceFactory;
            workspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory2)new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
            ESRI.ArcGIS.Geodatabase.IWorkspaceName worName = workspaceFactory.Create(path, shapefilename, null, 0);

            ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)worName;
            IWorkspace workspace = (ESRI.ArcGIS.Geodatabase.IWorkspace)name.Open();

            return workspace;
        }

        /// <summary>
        ///
        /// 功能描述:   Connecting to a transactional version ,others are Connecting to a historical marker name and Connecting to a historical time stamp
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="server"></param>
        /// <param name="instance"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static IWorkspace Open_ArcSDE_Workspace(string server, string instance, string user, string database, string password, string version)
        {
           try
           {
               ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
               propertySet.SetProperty("SERVER", server);
               propertySet.SetProperty("INSTANCE", instance);
               propertySet.SetProperty("DATABASE", database);
               propertySet.SetProperty("USER", user);
               propertySet.SetProperty("PASSWORD", password);
               propertySet.SetProperty("VERSION", version);

               IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();
               return workspaceFactory.Open(propertySet, 0);
           }
           catch (System.Exception ex)
           {
               // // LogHelper.LogHelper.("GeodatabaseOp.Open_ArcSDE_Workspace:" + ex.Message);
               System.Diagnostics.Debug.Write(ex.Message);
               return null;
           }
        }

        /// <summary>
        ///
        /// 功能描述:   从连接文件连接到ArcSde
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IWorkspace OpenFromFile_ArcSDE_Workspace(string connectionString)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();
            //The hWnd argument is the parent window or the application's window.
            //The hWnd will guarantee that the connection dialog box, if shown to you because of
            //insufficient properties, has the correct parent.

            return workspaceFactory.OpenFromFile(connectionString, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   打开Personal Geodatabase
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IWorkspace Open_pGDB_Workspace(string database)
        {
            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("DATABASE", database);
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();

            return workspaceFactory.Open(propertySet, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   通过连接文件打开Personal Geodatabase
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IWorkspace OpenFromFile_pGDB_Workspace(string connectionString)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();
            return workspaceFactory.OpenFromFile(connectionString, 0);


            //The last way to open a personal geodatabase is to utilize the OpenFromString method on IWorkspaceFactory2. The connection string used in this case is a collection of the name value pairs. For personal geodatabases, the collection only requires a name of DATABASE and a value of the path name to the workspace. See the following code example:

            //[C#]
            ////For example, connectionString = "DATABASE=C:\\myData\\mypGDB.mdb".
            //public static IWorkspace openFromString_pGDB_Workspace(string connectionString)
            //{
            //IWorkspaceFactory2 workspaceFactory;
            ////Explicitly cast during the cocreation of the workspace factory.
            //workspaceFactory = (IWorkspaceFactory2)new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();
            //return workspaceFactory.OpenFromString(connectionString, 0);
            //}
        }

        /// <summary>
        ///
        /// 功能描述:   打开file Geodatabase
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IWorkspace Open_fGDB_Workspace(string database)
        {
            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("DATABASE", database);
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();

            return workspaceFactory.Open(propertySet, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   通过连接文件打开file Geodatabase
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IWorkspace OpenFromFile_fGDB_Workspace(string connectionString)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();

            return workspaceFactory.OpenFromFile(connectionString, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   打开workgroup Geodatabase〔依赖于SQL Express〕
        ///             //For example, for direct connect with OSA authentication.
        ///             // Server = "tivo_sqlexpress".
        ///             // Database = "sewer".
        ///             // Instance = "sde:sqlserver:tivo\\sqlexpress" // Two back slashes because of C#.
        ///             // Authenticaion_mode = "OSA".
        ///             // Version = "dbo.DEFAULT".
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="server"></param>
        /// <param name="instance"></param>
        /// <param name="authentication_mode"></param>
        /// <param name="database"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static IWorkspace Open_Workgroup_ArcSDE_Workspace(string server, string instance, string authentication_mode, string database, string version)
        {
            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("SERVER", server);
            propertySet.SetProperty("INSTANCE", instance);
            propertySet.SetProperty("DATABASE", database);
            propertySet.SetProperty("AUTHENTICATION_MODE", authentication_mode);
            propertySet.SetProperty("VERSION", version);
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();

            return workspaceFactory.Open(propertySet, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   通过连接文件打开workgroup Geodatabase〔依赖于SQL Express〕
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IWorkspace OpenFromFile_Workgroup_ArcSDE_Workspace(string connectionString)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();
            //The hWnd argument is the parent window or application's window.
            //The hWnd will guarantee that the connection dialog box, if presented to you because of
            //insufficient properties, has the correct parent.

            return workspaceFactory.OpenFromFile(connectionString, 0);
        }


        /// <summary>
        ///
        /// 功能描述:   打开workgroup Geodatabase
        ///             The following code example shows how to connect to a transactional version of a geodatabase stored in a DataServer
        ///             for a personal or workgroup ArcSDE using the DataServerManager:
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="serverName">"tivo\\sqlexpress"</param>
        /// <returns></returns>
        public static IWorkspace Open_Workgroup_ArcSDE_Workspace(string serverName, string database)
        {
            // Create a Data Server Manager object.
            ESRI.ArcGIS.DataSourcesGDB.IDataServerManager dataserverManager = new ESRI.ArcGIS.DataSourcesGDB.DataServerManagerClass();

            // Set the server name and connect to the server.
            dataserverManager.ServerName = serverName;
            dataserverManager.Connect();

            // Open one of the geodatabases in the Database Server.
            ESRI.ArcGIS.DataSourcesGDB.IDataServerManagerAdmin dataservermanagerAdmin = (ESRI.ArcGIS.DataSourcesGDB.IDataServerManagerAdmin)dataserverManager;
            IWorkspaceName workspaceName = dataservermanagerAdmin.CreateWorkspaceName(database, "VERSION", "dbo.Default");

            // Cast from the workspace name to utilize the Open method.
            ESRI.ArcGIS.esriSystem.IName name = (ESRI.ArcGIS.esriSystem.IName)workspaceName;
            IWorkspace workspace = (IWorkspace)name.Open();

            return workspace;
        }

        /// <summary>
        ///
        /// 功能描述:   打开shapefile
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IWorkspace Open_shapefile_Workspace(string database)
        {
            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("DATABASE", database);
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();

            return workspaceFactory.Open(propertySet, 0);
        }

        public static IFeatureClass OpenShapefileAsFeatClass(string fullName)
        {
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                IFeatureWorkspace featWorkspace = Open_shapefile_Workspace(database) as IFeatureWorkspace;
                IFeatureClass featureClass = featWorkspace.OpenFeatureClass(datasetname);

                return featureClass;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenShapefileAsFeatClass:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IFeatureLayer OpenShapefileLayer(string fullName)
        {
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                IFeatureWorkspace featWorkspace = Open_shapefile_Workspace(database) as IFeatureWorkspace;
                IFeatureClass featureClass  = featWorkspace.OpenFeatureClass(datasetname);
                IDataset dataset = featureClass as IDataset;

                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.FeatureClass = featureClass;
                featureLayer.Name = String.Format("{0}\\{1}", dataset.Workspace.PathName, dataset.BrowseName);

                return featureLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenShapefileLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   通过连接文件打开shapefile
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IWorkspace OpenFromString_shapefile_Workspace(string connectionString)
        {
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
            return workspaceFactory.OpenFromFile(connectionString, 0);
        }


        public static IWorkspace Open_CAD_Workspace(string database)
        {
            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("DATABASE", database);

            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesFile.CadWorkspaceFactoryClass();

            return workspaceFactory.Open(propertySet, 0);
        }

        public static ICadDrawingDataset OpenCadDrawingDataset(string fullName)
        {
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                ICadDrawingWorkspace cadWorkspace = Open_CAD_Workspace(database) as ICadDrawingWorkspace;
                ICadDrawingDataset cadDrawingDataset = cadWorkspace.OpenCadDrawingDataset(datasetname);

                return cadDrawingDataset;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenCadDrawingDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static ICadLayer OpenCADLayer(string fullName)
        {
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                ICadDrawingWorkspace cadWorkspace = Open_CAD_Workspace(database) as ICadDrawingWorkspace;
                ICadDrawingDataset cadDrawingDataset = cadWorkspace.OpenCadDrawingDataset(datasetname);

                IDataset dataset = cadDrawingDataset as IDataset;

                ICadLayer cadLayer = new CadLayerClass();
                cadLayer.CadDrawingDataset = cadDrawingDataset;
                cadLayer.Name = String.Format("{0}\\{1}", dataset.Workspace.PathName, dataset.BrowseName);

                return cadLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenCADLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }


        public static ILayer OpenLayerFromLayerFile(string layerFilePath)
        {
            try
            {
                //Create a new LayerFile instance.
                ESRI.ArcGIS.Carto.ILayerFile layerFile = new ESRI.ArcGIS.Carto.LayerFileClass();
                layerFile.Open(layerFilePath);

                return layerFile.Layer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenLayerFromLayerFile:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   打开表
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static ITable OpenTable(IWorkspace workspace, string tablename)
        {
            IFeatureWorkspace featWs = workspace as IFeatureWorkspace;
            if (featWs == null)
                return null;

            return featWs.OpenTable(tablename);
        }

        /// <summary>
        ///
        /// 功能描述:   创建表
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="tablename"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static ITable CreateTable(IWorkspace workspace, string tablename,IFields fields)
        {
            if (workspace == null)
            {
                return null;
            }

            ITable retValue = null;
            UID CLSID;

            CLSID = new UID();
            CLSID.Value = "esriGeoDatabase.Object";
            try
            {
                IFeatureWorkspace ipfeatWS = workspace as IFeatureWorkspace;
                if (ipfeatWS == null)
                {
                    return null;
                }

                retValue = ipfeatWS.CreateTable(tablename, fields, CLSID, null, null);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.CreateTable:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n创建表[" + tablename + "]失败");
                //return false;
            }

            return retValue;
        }

        /// <summary>
        ///
        /// 功能描述:   删除表
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static bool DeleteTable(IWorkspace workspace, string tablename)
        {
            ITable table = GeodatabaseOp.OpenTable(workspace, tablename);
            if (table == null)
            {
                return true;
            }

            IDataset dataset = table as IDataset;
            if (dataset == null)
            {
                return false;
            }

            try
            {
                string sTabName = "";
                string sUser = "";

                if (GeodatabaseOp.CheckHasLock(table as ISchemaLock, ref sTabName, ref sUser))
                {
                    // // LogHelper.LogHelper.("DeleteTable:表[" + sTabName + "]该图库被用户[" + sUser + "]锁定，无法对其进行删除操作。");
                    throw new ApplicationException("表[" + sTabName + "]该图库被用户[" + sUser + "]锁定，无法对其进行删除操作。");
                }
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteTable:" + ex.Message);
                //throw ex;
            }


            dataset.Delete();

            return true;
        }

        /// <summary>
        ///
        /// 功能描述:   删除FeatureClass
        /// 开发者:     XXX
        /// 建立时间:   2008-10-13 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="featureClass"></param>
        /// <returns></returns>
        public static bool DeleteFeatureClass(IWorkspace workspace,string featureClass)
        {
            IFeatureClass featClass = GeodatabaseOp.OpenFeatClass(workspace, featureClass);
            if (featClass == null)
            {
                return true;
            }

            IDataset dataset = featClass as IDataset;
            if (dataset == null)
            {
                return false;
            }

            try
            {
                string sTabName = "";
                string sUser = "";

                if (GeodatabaseOp.CheckFeatureClassHasLock(featClass, ref sTabName, ref sUser))
                {
                    // // LogHelper.LogHelper.("DeleteFeatureClass:FeatureClass[" + sTabName + "]该图库被用户[" + sUser + "]锁定，无法对其进行删除操作。");
                    throw new ApplicationException("FeatureClass[" + sTabName + "]该图库被用户[" + sUser + "]锁定，无法对其进行删除操作。");
                }
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteFeatureClass:" + ex.Message);
                //throw ex;
            }

            dataset.Delete();

            return true;
        }

        /// <summary>
        ///
        /// 功能描述:   打开文件类型的栅格
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static IRasterDataset OpenFileRasterDataset(string folderName, string datasetName)
        {
            //Open raster file workspace.
            IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesRaster.RasterWorkspaceFactoryClass();
            ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace rasterWorkspace = (ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace)workspaceFactory.OpenFromFile(folderName, 0);

            //Open file raster dataset.
            IRasterDataset rasterDataset = rasterWorkspace.OpenRasterDataset(datasetName);

            return rasterDataset;
        }

        /// <summary>
        ///
        /// 功能描述:   打开GDB类型栅格数据
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="rasterWorkspaceEx"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static IRasterDataset OpenGDBRasterDataset(IRasterWorkspaceEx rasterWorkspaceEx, string datasetName)
        {
            //Open a raster dataset in a geodatabase including PGDB, FGDB, and ArcSDE.
            return rasterWorkspaceEx.OpenRasterDataset(datasetName);
        }

        /// <summary>
        ///
        /// 功能描述:   打开GDB类型栅格数据
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="datasetName"></param>
        /// <returns></returns>
        public static IRasterDataset OpenGDBRasterDataset(IWorkspace workspace, string datasetName)
        {
            IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
            if (rasterWorkspaceEx == null)
            {
                return null;
            }

            //Open a raster dataset in a geodatabase including PGDB, FGDB, and ArcSDE.
            return rasterWorkspaceEx.OpenRasterDataset(datasetName);
        }

        /// <summary>
        ///
        /// 功能描述:   打开RasterCatalog
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="rasterWorkspaceEx"></param>
        /// <param name="catalogName"></param>
        /// <returns></returns>
        public static IRasterCatalog OpenRasterCatalog(IRasterWorkspaceEx rasterWorkspaceEx, string catalogName)
        {
            //Open a raster catalog in a geodatabase including PGDB, FGDB, and ArcSDE.
            return rasterWorkspaceEx.OpenRasterCatalog(catalogName);
        }

        /// <summary>
        ///
        /// 功能描述:   打开RasterCatalog
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="catalogName"></param>
        /// <returns></returns>
        public static IRasterCatalog OpenRasterCatalog(IWorkspace workspace, string catalogName)
        {
            IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
            if (rasterWorkspaceEx == null)
            {
                return null;
            }

            //Open a raster catalog in a geodatabase including PGDB, FGDB, and ArcSDE.
            return rasterWorkspaceEx.OpenRasterCatalog(catalogName);
        }

        /// <summary>
        ///
        /// 功能描述:   打开RasterCatalog中某一项的栅格数据
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static IRasterDataset GetRasterCatalogItem(IRasterCatalog catalog, int oid)
        {
             //OID is the ObjectID of the raster dataset in the raster catalog.
             IFeatureClass featureClass = (IFeatureClass)catalog;
             IRasterCatalogItem rasterCatalogItem = (IRasterCatalogItem)featureClass.GetFeature(oid);

             return rasterCatalogItem.RasterDataset;
        }

        /// <summary>
        ///
        /// 功能描述:   Open file geodatabase workspace as RasterWorkspace.
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="FGDBPath"></param>
        /// <returns></returns>
        public static IRasterWorkspaceEx OpenFGDB(string FGDBPath)
        {
            //FGDBPath string example: c:\data\raster.gdb.
            IWorkspaceFactory2 workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();

            return (IRasterWorkspaceEx)workspaceFactory.OpenFromFile(FGDBPath, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   Open ArcSDE workspace as RasterWorkspace.
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="conString"></param>
        /// <returns></returns>
        public static IRasterWorkspaceEx OpenSDE(string conString)
        {
            //conString example: SERVER=qian;INSTANCE=9200;VERSION=sde.DEFAULT;USER=raster;PASSWORD=raster.
            IWorkspaceFactory2 workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.SdeWorkspaceFactoryClass();

            return (IRasterWorkspaceEx)workspaceFactory.OpenFromString(conString, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   Open accessed workspace as RasterWorkspace.
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="PGDBPath"></param>
        /// <returns></returns>
        public static IRasterWorkspaceEx OpenAccess(string PGDBPath)
        {
            //FGDBPath string example: c:\data\rasters.mdb.
            IWorkspaceFactory2 workspaceFactory = new ESRI.ArcGIS.DataSourcesGDB.AccessWorkspaceFactoryClass();

            return (IRasterWorkspaceEx)workspaceFactory.OpenFromFile(PGDBPath, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   Open file workspace as RasterWorkspace.
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="wsPath"></param>
        /// <returns></returns>
        public static ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace OpenRasterFileWorkspace(string wsPath)
        {
            //wsPath example: c:\data\rasters.
            IWorkspaceFactory workspaceFact = new ESRI.ArcGIS.DataSourcesRaster.RasterWorkspaceFactoryClass();

            return (ESRI.ArcGIS.DataSourcesRaster.IRasterWorkspace)workspaceFact.OpenFromFile(wsPath, 0);
        }

        /// <summary>
        ///
        /// 功能描述:   创建IRasterCatalog:几何字段和栅格字段名称固定:Shape 和 Raster
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="rasterWorkspaceEx"></param>
        /// <param name="catalogName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IRasterCatalog CreateRasterCatalog(IRasterWorkspaceEx rasterWorkspaceEx, string catalogName,IFields fields)
        {
            return rasterWorkspaceEx.CreateRasterCatalog(catalogName, fields, "Shape", "Raster", "defaults");
        }

        /// <summary>
        ///
        /// 功能描述:   创建IRasterCatalog;几何字段和栅格字段名称固定:Shape 和 Raster
        /// 开发者:     XXX
        /// 建立时间:   2008-10-14 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="catalogName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IRasterCatalog CreateRasterCatalog(IWorkspace workspace, string catalogName, IFields fields)
        {
            IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
            if (rasterWorkspaceEx == null)
            {
                return null;
            }

            return rasterWorkspaceEx.CreateRasterCatalog(catalogName, fields, "Shape", "Raster", "");
        }

        /// <summary>
        ///
        /// 功能描述:   删除IRasterCatalog
        /// 开发者:     XXX
        /// 建立时间:   2008-10-17 0:00:00
        ///
        /// </summary>
        /// <param name="rasterWorkspaceEx"></param>
        /// <param name="catalogName"></param>
        public static bool DeleteRasterCatalog(IRasterWorkspaceEx rasterWorkspaceEx, string catalogName)
        {
            try
            {
                try
                {
                    IRasterCatalog rc = GeodatabaseOp.OpenRasterCatalog(rasterWorkspaceEx, catalogName);
                    if (rc == null)
                    {
                        return true;
                    }
                }
                catch (System.Exception ex2)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRasterCatalog:" + ex2.Message);
                    System.Diagnostics.Debug.Write(ex2.Message);
                     return true;
                }

                rasterWorkspaceEx.DeleteRasterCatalog(catalogName);

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRasterCatalog:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   删除IRasterCatalog
        /// 开发者:     XXX
        /// 建立时间:   2008-10-17 0:00:00
        ///
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="catalogName"></param>
        public static bool DeleteRasterCatalog(IWorkspace workspace, string catalogName)
        {
            IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
            if (rasterWorkspaceEx == null)
            {
                return false;
            }

            return GeodatabaseOp.DeleteRasterCatalog(rasterWorkspaceEx, catalogName);
        }


        /// <summary>
        ///
        /// 功能描述:   根据条件删除表中数据
        /// 开发者:     XXX
        /// 建立时间:   2008-10-17 0:00:00
        ///
        /// </summary>
        /// <param name="table"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static bool DeleteRowsInTable(ITable table ,string whereClause)
        {
            try
            {
                if (table == null)
                {
                    return false;
                }

                IQueryFilter queryfilter = new QueryFilterClass();
                queryfilter.WhereClause = whereClause;

                if (table.RowCount(queryfilter) < 1)
                {
                    return true;
                }

                table.DeleteSearchedRows(queryfilter);

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRowsInTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static IRasterDataset OpenRasterDataset(string fullName)
        {
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                IRasterWorkspace2 rasterWorkspace2 = OpenRasterFileWorkspace(database) as IRasterWorkspace2;
                IRasterDataset rasterDataset = rasterWorkspace2.OpenRasterDataset(datasetname);

                return rasterDataset;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenRasterDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IRasterLayer OpenRasterLayer(string fullName)
        {
            IRasterLayer rasterLayer = new RasterLayerClass();
            try
            {
                string database = fullName.Substring(0, fullName.LastIndexOf('\\'));
                string datasetname = fullName.Substring(fullName.LastIndexOf('\\') + 1);

                IRasterWorkspace2 rasterWorkspace2 = OpenRasterFileWorkspace(database) as IRasterWorkspace2;
                IRasterDataset rasterDataset = rasterWorkspace2.OpenRasterDataset(datasetname);
                IDataset dataset = rasterDataset as IDataset;

                rasterLayer.CreateFromDataset(rasterDataset);
                rasterLayer.Name = String.Format("{0}\\{1}", dataset.Workspace.PathName, dataset.BrowseName);

                return rasterLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenRasterLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                try
                {
                    rasterLayer.CreateFromFilePath(fullName);
                    rasterLayer.Name = fullName;

                    return rasterLayer;
                }
                catch (System.Exception ex2)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.OpenRasterLayer:" + ex2.Message);
                    System.Diagnostics.Debug.Write(ex2.Message);
                    return null;
                }
            }
        }

        public static IRasterLayer OpenRasterLayer(IWorkspace workspace,string layername)
        {
            IRasterWorkspaceEx rasterworkspaceEx = workspace as IRasterWorkspaceEx;
            return OpenRasterLayer(rasterworkspaceEx, layername);
        }

        public static IRasterLayer OpenRasterLayer(IRasterWorkspaceEx rasterworkspaceEx,string layername)
        {
            try
            {
                if (rasterworkspaceEx == null)
                {
                    return null;
                }

                IRasterDataset rasterdataset = rasterworkspaceEx.OpenRasterDataset(layername);
                if (rasterdataset == null)
                {
                    return null;
                }

                IRasterLayer rasterLayer = new RasterLayerClass();
                rasterLayer.CreateFromDataset(rasterdataset);

                return rasterLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenRasterLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IRasterLayer GetRasterCatalogItem(IWorkspace workspace,string catalogName,string layername)
        {
            IRasterWorkspaceEx rasterworkspaceEx = workspace as IRasterWorkspaceEx;
            return GetRasterCatalogItem(rasterworkspaceEx, catalogName, layername);
        }

        public static IRasterDataset GetRasterCatalogItemRasterDataset(IWorkspace workspace, string catalogName, string layername)
        {
            return GetRasterCatalogItemRasterDataset(workspace as IRasterWorkspaceEx, catalogName, layername);
        }

        public static IRasterDataset GetRasterCatalogItemRasterDataset(IRasterWorkspaceEx rasterworkspaceEx, string catalogName, string layername)
        {
            try
            {
                if (rasterworkspaceEx == null)
                {
                    return null;
                }

                IRasterCatalog catalog = OpenRasterCatalog(rasterworkspaceEx, catalogName);
                if (catalog == null)
                {
                    return null;
                }

                ITable table = catalog as ITable;
                IQueryFilter queryfilter = new QueryFilterClass();
                queryfilter.WhereClause = String.Format("Name = '{0}'", layername);
                ICursor cursor = table.Search(queryfilter, true);
                IRow row = cursor.NextRow();

                IRasterDataset rasterdataset = null;

                while (row != null)
                {
                    int rowid = System.Convert.ToInt32(row.get_Value(table.FindField(table.OIDFieldName)));
                    rasterdataset = GetRasterCatalogItem(catalog, rowid);
                    if (rasterdataset != null)
                    {
                        break;
                    }

                    row = cursor.NextRow();
                }

                return rasterdataset;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterCatalogItemRasterDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IRasterLayer GetRasterCatalogItem(IRasterWorkspaceEx rasterworkspaceEx, string catalogName, string layername)
        {
            try
            {
                if (rasterworkspaceEx == null)
                {
                    return null;
                }

                IRasterCatalog catalog = OpenRasterCatalog(rasterworkspaceEx, catalogName);
                if (catalog == null)
                {
                    return null;
                }

                ITable table = catalog as ITable;
                IQueryFilter queryfilter = new QueryFilterClass();
                queryfilter.WhereClause = String.Format("Name = '{0}'", layername);
                ICursor cursor = table.Search(queryfilter, true);
                IRow row = cursor.NextRow();

                IRasterDataset rasterdataset = null;

                while (row != null)
                {
                    int rowid = System.Convert.ToInt32(row.get_Value(table.FindField(table.OIDFieldName)));
                    rasterdataset = GetRasterCatalogItem(catalog, rowid);
                    if (rasterdataset != null)
                    {
                        break;
                    }

                    row = cursor.NextRow();
                }

                IRasterLayer rasterLayer = new RasterLayerClass();
                rasterLayer.CreateFromDataset(rasterdataset);

                return rasterLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterCatalogItem:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static DataTable EsriTableConvertToDataTable(ITable table,string whereClause)
        {
            try
            {
                if (table == null)
                {
                    return null;
                }

                //因为获取Blob字段数据比较耗时，这里预览数据时候暂不获取数据，在具体单击浏览再获取
                StringBuilder subfieldsbuilder = null;
                DataTable datatable = null;

                if (!ConvertEsriTableToDataTable(table, null, out datatable, out subfieldsbuilder))
                {
                    return null;
                }

                if (!ConvertEsriTableDataToDataTableData(table, datatable, subfieldsbuilder.ToString(), whereClause))
                {
                    return null;
                }

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.EsriTableConvertToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static bool ConvertEsriTableToDataTable(ITable table,string[] SubFieldsArray,out DataTable datatable,out  StringBuilder subFieldsBuilder)
        {
            subFieldsBuilder = null;
            datatable = null;

            try
            {
                string dataName = (table as IDataset).Name;
                datatable = new DataTable(dataName);

                //因为获取Blob字段数据比较耗时，这里预览数据时候暂不获取数据，在具体单击浏览再获取
                IField field = null;
                int fieldCount = 0;
                int index = -1;

                if (SubFieldsArray == null || SubFieldsArray.Length < 1)
                {
                    fieldCount = table.Fields.FieldCount;
                }
                else
                {
                    fieldCount = SubFieldsArray.Length;
                }

                subFieldsBuilder = new StringBuilder();
                DataColumn dataColumns = null;
                string sstemp;

                for (int i = 0; i < fieldCount; i++)
                {
                    dataColumns = new DataColumn();

                    if (SubFieldsArray == null || SubFieldsArray.Length < 1)
                    {
                        field = table.Fields.get_Field(i);
                    }
                    else
                    {
                        index = table.FindField(SubFieldsArray[i]);
                        if (index < 0)
                        {
                            continue;
                        }

                        field = table.Fields.get_Field(index);
                    }

                    dataColumns.ColumnName = field.Name.ToUpper();
                    dataColumns.Caption = field.AliasName.ToUpper();

                    datatable.Columns.Add(dataColumns);

                    if (field.Type != esriFieldType.esriFieldTypeBlob && field.Type != esriFieldType.esriFieldTypeGeometry && field.Type != esriFieldType.esriFieldTypeRaster)
                    {
                        if (subFieldsBuilder.Length == 0)
                        {
                            sstemp = field.Name;
                        }
                        else
                        {
                            sstemp = String.Format(",{0}", field.Name);
                        }
                        subFieldsBuilder.Append(sstemp);
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertEsriTableToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static DataTable EsriTableConvertToDataTable(ITable table, ICursor cursor)
        {
            try
            {
                if (table == null)
                {
                    return null;
                }

                //因为获取Blob字段数据比较耗时，这里预览数据时候暂不获取数据，在具体单击浏览再获取
                StringBuilder subfieldsbuilder = null;
                DataTable datatable = null;

                if (!ConvertEsriTableToDataTable(table, null, out datatable, out subfieldsbuilder))
                {
                    return null;
                }

                if (!ConvertEsriTableDataToDataTableData(table, datatable, cursor))
                {
                    return null;
                }

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.EsriTableConvertToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static bool ConvertEsriTableDataToDataTableData(ITable table, DataTable datatable,ICursor cursor)
        {
            try
            {
                DataRow datarow = null;
                int index = -1;

                IRow row = cursor.NextRow();
                while (row != null)
                {
                    datarow = datatable.NewRow();
                    for (int i = 0; i < datatable.Columns.Count; i++)
                    {
                        index = table.FindField(datatable.Columns[i].ColumnName);
                        if (index < 0)
                        {
                            continue;
                        }

                        if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeBlob)
                        {
                            datarow[i] = "Blob";
                        }
                        else if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            datarow[i] = "Shape";
                        }
                        else if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeRaster)
                        {
                            datarow[i] = "Raster";
                        }
                        else
                        {
                            datarow[i] = row.get_Value(index);
                        }
                    }

                    datatable.Rows.Add(datarow);

                    row = cursor.NextRow();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertEsriTableDataToDataTableData:" + ex.Message);
                throw ex;
            }
        }

        public static bool ConvertEsriTableDataToDataTableData(ITable table,DataTable datatable,string subFields,string whereClause)
        {
            try
            {
                IQueryFilter queryfilter = new QueryFilterClass();
                queryfilter.WhereClause = whereClause;
                queryfilter.SubFields = subFields;

                DataRow datarow = null;
                int index = -1;

                ICursor cursor = table.Search(queryfilter, true);
                IRow row = cursor.NextRow();
                while (row != null)
                {
                    datarow = datatable.NewRow();
                    for (int i = 0; i < datatable.Columns.Count; i++)
                    {
                        index = table.FindField(datatable.Columns[i].ColumnName);
                        if (index < 0)
                        {
                            continue;
                        }

                        if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeBlob)
                        {
                            datarow[i] = "Blob";
                        }
                        else if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            datarow[i] = "Shape";
                        }
                        else if (table.Fields.get_Field(index).Type == esriFieldType.esriFieldTypeRaster)
                        {
                            datarow[i] = "Raster";
                        }
                        else
                        {
                            datarow[i] = row.get_Value(index);
                        }
                    }

                    datatable.Rows.Add(datarow);

                    row = cursor.NextRow();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertEsriTableDataToDataTableData:" + ex.Message);
                throw ex;
            }
        }

        public static DataTable EsriTableConvertToDataTable(ITable table,string subFields, string whereClause)
        {
            try
            {
                if (table == null)
                {
                    return null;
                }

                if (subFields.Equals("*"))
                {
                    return EsriTableConvertToDataTable(table, whereClause);
                }

                //因为获取Blob字段数据比较耗时，这里预览数据时候暂不获取数据，在具体单击浏览再获取
                StringBuilder subfieldsbuilder = null;
                DataTable datatable = null;

                string[] subFieldsArray = subFields.Split(new char[] { ',' });
                if (!ConvertEsriTableToDataTable(table, subFieldsArray, out datatable, out subfieldsbuilder))
                {
                    return null;
                }

                if (!ConvertEsriTableDataToDataTableData(table, datatable, subfieldsbuilder.ToString(), whereClause))
                {
                    return null;
                }

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.EsriTableConvertToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }


        //IFeatureDataConverter ConvertFeatureClass Example
        //e.g., nameOfSourceFeatureClass = "ctgFeatureshp.shp"
        //      nameOfTargetFeatureClass = "ctgFeature"
        public static void IFeatureDataConverter_ConvertFeatureClass_Example(IWorkspace sourceWorkspace, IWorkspace targetWorkspace, string nameOfSourceFeatureClass, string nameOfTargetFeatureClass)
        {
            //create source workspace name
            IDataset sourceWorkspaceDataset = (IDataset)sourceWorkspace;
            IWorkspaceName sourceWorkspaceName = (IWorkspaceName)sourceWorkspaceDataset.FullName;

            //create source dataset name
            IFeatureClassName sourceFeatureClassName = new FeatureClassNameClass();
            IDatasetName sourceDatasetName = (IDatasetName)sourceFeatureClassName;
            sourceDatasetName.WorkspaceName = sourceWorkspaceName;
            sourceDatasetName.Name = nameOfSourceFeatureClass;

            //create target workspace name
            IDataset targetWorkspaceDataset = (IDataset)targetWorkspace;
            IWorkspaceName targetWorkspaceName = (IWorkspaceName)targetWorkspaceDataset.FullName;

            //create target dataset name
            IFeatureClassName targetFeatureClassName = new FeatureClassNameClass();
            IDatasetName targetDatasetName = (IDatasetName)targetFeatureClassName;
            targetDatasetName.WorkspaceName = targetWorkspaceName;
            targetDatasetName.Name = nameOfTargetFeatureClass;

            //Open input Featureclass to get field definitions.
            ESRI.ArcGIS.esriSystem.IName sourceName = (ESRI.ArcGIS.esriSystem.IName)sourceFeatureClassName;
            IFeatureClass sourceFeatureClass = (IFeatureClass)sourceName.Open();

            //Validate the field names because you are converting between different workspace types.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IFields targetFeatureClassFields;
            IFields sourceFeatureClassFields = sourceFeatureClass.Fields;
            IEnumFieldError enumFieldError;

            // Most importantly set the input and validate workspaces!
            fieldChecker.InputWorkspace = sourceWorkspace;
            fieldChecker.ValidateWorkspace = targetWorkspace;
            fieldChecker.Validate(sourceFeatureClassFields, out enumFieldError, out targetFeatureClassFields);

            // Loop through the output fields to find the geomerty field
            IField geometryField;
            for (int i = 0; i < targetFeatureClassFields.FieldCount; i++)
            {
                if (targetFeatureClassFields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                {
                    geometryField = targetFeatureClassFields.get_Field(i);

                    // Get the geometry field's geometry defenition
                    IGeometryDef geometryDef = geometryField.GeometryDef;

                    //Give the geometry definition a spatial index grid count and grid size
                    IGeometryDefEdit targetFCGeoDefEdit = (IGeometryDefEdit)geometryDef;
                    targetFCGeoDefEdit.GridCount_2 = 1;
                    targetFCGeoDefEdit.set_GridSize(0, 0);

                    //Allow ArcGIS to determine a valid grid size for the data loaded
                    targetFCGeoDefEdit.SpatialReference_2 = geometryField.GeometryDef.SpatialReference;

                    // we want to convert all of the features
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = "";

                    // Load the feature class
                    IFeatureDataConverter fctofc = new FeatureDataConverterClass();
                    IEnumInvalidObject enumErrors = fctofc.ConvertFeatureClass(sourceFeatureClassName, queryFilter, null, targetFeatureClassName, geometryDef, targetFeatureClassFields, "", 1000, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// 30°41′00.35″	121°14′55.80″
        /// </summary>
        /// <param name="sDMS"></param>
        /// <returns></returns>
        public static double ConvertDMSToDD(string sDMS)
        {
            string degree = sDMS.Substring(0, sDMS.IndexOf("°"));
            string min = sDMS.Substring(sDMS.IndexOf("°") + 1, sDMS.IndexOf("′") - sDMS.IndexOf("°") - 1);
            string sec = sDMS.Substring(sDMS.IndexOf("′") + 1, sDMS.IndexOf("″") - sDMS.IndexOf("′") - 1);

            double ddegree = Double.Parse(degree) + Double.Parse(min)/60.00 + Double.Parse(sec) / 3600.00;

            return ddegree;
        }

        public static string ConvertDMSToString(double dd)
        {
            try
            {
                int degree = 120;
                int minute = 10;
                double second = 0.0;

                ConvertDegree2DMS(dd, ref degree, ref minute, ref second);

                return String.Format("{0}°{1}′{2:F2}″", degree, minute, second);
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertDMSToString:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return String.Empty;
            }
        }

        public static bool DeleteRasterDataset(IWorkspace ws ,string rasterdatasetName)
        {
            IRasterWorkspaceEx rasterWsEx = ws as IRasterWorkspaceEx;

            return DeleteRasterDataset(rasterWsEx, rasterdatasetName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rasterWsEx"></param>
        /// <param name="rasterdatasetName"></param>
        /// <returns></returns>
        public static bool DeleteRasterDataset(IRasterWorkspaceEx rasterWsEx ,string rasterdatasetName)
        {
            try
            {
                if (rasterWsEx == null)
                {
                    return false;
                }

                IRasterDataset rasterdataset  = null;
                try
                {
                    rasterdataset  = rasterWsEx.OpenRasterDataset(rasterdatasetName);
                }
                catch (System.Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRasterDataset:" + ex.Message);
                    System.Diagnostics.Debug.Write(ex.Message);
                    return true;
                }

                IDataset dataset = rasterdataset as IDataset;
                if (dataset.CanDelete())
                {
                    dataset.Delete();
                    return true;
                }

                return false;

            }
            catch (System.Exception ex2)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRasterDataset:" + ex2.Message);
                System.Diagnostics.Debug.Write(ex2.Message);
                return false;
            }
        }


        public static IFeatureClass CreateSimilarFeatureClass(IWorkspace workspace,string sourcelayerName,out string newLayerName,out string newLayerALias)
        {
            newLayerName = String.Empty;
            newLayerALias = String.Empty;
            try
            {
                IFeatureClass sourceFeatureClass = GeodatabaseOp.OpenFeatClass(workspace, sourcelayerName);
                 if (sourceFeatureClass == null)
                 {
                     return null;
                 }

                //克隆字段
                IClone colne = sourceFeatureClass.Fields as IClone;
                IClone newClone = colne.Clone();
                IFields fields = newClone as IFields;

                //图层名称:使用随机数填充，补齐6位形成新图层名称
                Random rdn = new Random();
                string newPart = rdn.Next(10000).ToString();
                newPart = newPart.PadLeft(4, '0');

                newLayerName = String.Format("{0}_S{1}", sourcelayerName, newPart);

                //创建FeatureClass
                IFeatureClass newFeatClass = null;
                if (sourceFeatureClass.FeatureDataset != null)
                {
                    newFeatClass = CreateFeatureClass(sourceFeatureClass.FeatureDataset, newLayerName,fields);
                }
                else
                {
                    IFeatureWorkspace featWorkspace = workspace as IFeatureWorkspace;

                    newFeatClass = CreateFeatureClass(featWorkspace, newLayerName, fields);
                }

                newLayerALias = String.Format("{0}_相似{1}", sourceFeatureClass.AliasName, newPart);
                GeodatabaseOp.SetSysFieldAlias(newFeatClass, newLayerALias);

                return newFeatClass;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.CreateSimilarFeatureClass:" + ex.Message);
            	System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IList<string> GetRasterCatalogItemNames(IRasterWorkspaceEx rasterWsEx ,string rasterCatalogName)
        {
            try
            {
                IList<string> ItemNames = new List<string>();
                string sName = String.Empty;

                IRasterCatalog rasterCatalog = OpenRasterCatalog(rasterWsEx, rasterCatalogName);
                ITable table = rasterCatalog as ITable;
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.SubFields = "Name";
                ICursor cursor = table.Search(queryFilter, false);
                IRow rowss = cursor.NextRow();
                while (rowss != null)
                {
                    sName = rowss.get_Value(rasterCatalog.NameFieldIndex).ToString();

                    ItemNames.Add(sName);
                    rowss = cursor.NextRow();
                }

                return ItemNames;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterCatalogItemNames:" + ex.Message);
                throw ex;
            }
        }

        public static IList<string> GetRasterCatalogItemNames(IWorkspace workspace, string rasterCatalogName)
        {
           try
           {
               IRasterWorkspaceEx rasterWsex = workspace as IRasterWorkspaceEx;
               return GetRasterCatalogItemNames(rasterWsex, rasterCatalogName);
           }
           catch (System.Exception ex)
           {
               // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterCatalogItemNames:" + ex.Message);
               System.Diagnostics.Debug.Write(ex.Message);
               return null;
           }
        }


        public static bool DeleteRasterCatalogItem(IWorkspace workspace, string rasterCatalogName,string whereClause)
        {
            return DeleteRasterCatalogItem(workspace as IRasterWorkspaceEx, rasterCatalogName, whereClause);
        }

        public static bool DeleteRasterCatalogItem(IRasterWorkspaceEx rasterWorkspaceEx, string rasterCatalogName,string whereClause)
        {
            ITransactions transaction = rasterWorkspaceEx as ITransactions;
            try
            {
                if (rasterWorkspaceEx == null)
                {
                    return false;
                }

                if (transaction.InTransaction)
                {
                    return false;
                }

                transaction.StartTransaction();

                IRasterCatalog rasterCatalog = OpenRasterCatalog(rasterWorkspaceEx, rasterCatalogName);
                ITable table = rasterCatalog as ITable;
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = whereClause;
                ICursor cursor = table.Search(queryFilter, false);
                IRow rowss = cursor.NextRow();
                while (rowss != null)
                {
                    rowss.Delete();

                    rowss = cursor.NextRow();
                }

                transaction.CommitTransaction();

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DeleteRasterCatalogItem:" + ex.Message);
                transaction.AbortTransaction();
                throw ex;
            }
        }


        public static ITinWorkspace OpenTinFromFile(string tinPath)
        {
            IWorkspaceFactory wf = new TinWorkspaceFactoryClass();

            ESRI.ArcGIS.esriSystem.IPropertySet propertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
            propertySet.SetProperty("DATABASE", tinPath);

            return wf.Open(propertySet, 0) as ITinWorkspace;
        }

        public static bool DeleteTinFile(string tinName)
        {
           try
           {
               string tinPath = tinName.Substring(0, tinName.LastIndexOf("\\"));
               string name = tinName.Substring(tinName.LastIndexOf("\\") + 1);

               ITinWorkspace tinWS = OpenTinFromFile(tinPath);
               ITin tin = OpenTinFile(tinWS, name);
               if (tin == null)
               {
                   return true;
               }

               IDataset dataset = tin as IDataset;
               dataset.Delete();

               return true;
           }
           catch (System.Exception ex)
           {
               // // LogHelper.LogHelper.("GeodatabaseOp.DeleteTinFile:" + ex.Message);
               System.Diagnostics.Debug.Write(ex.Message);
               return false;
           }
        }

        public static ITin OpenTinFile(ITinWorkspace tinWS,string tinName)
        {
            try
            {
                ITin tin = tinWS.OpenTin(tinName);
                return tin;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenTinFile:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static ITin OpenTin(string tinFullName)
        {
            try
            {
                string path = tinFullName.Substring(0, tinFullName.LastIndexOf("\\"));
                string name = tinFullName.Substring(tinFullName.LastIndexOf("\\") + 1);

                return OpenTinFile(OpenTinFromFile(path), name);
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenTin:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static ITinLayer OpenTinlayer(string tinFullName)
        {
            try
            {
                ITin tin = OpenTin(tinFullName);

                ITinLayer tinLayer = new TinLayerClass();

                tinLayer.Dataset = tin;
                tinLayer.Name = (tin as IDataset).BrowseName;

                return tinLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenTinlayer:" + ex.Message);
            	System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IRasterLayer OpenRasterCatalogItemLayer(IWorkspace ws,string itemname,string catalogname)
        {
            return OpenRasterCatalogItemLayer(ws as IRasterWorkspaceEx, itemname, catalogname);
        }

        public static IRasterLayer OpenRasterCatalogItemLayer(IRasterWorkspaceEx rasterWsEx, string itemname, string catalogname)
        {
            try
            {
                IRasterLayer rasterLayer = new RasterLayerClass();
                rasterLayer.CreateFromDataset(GetRasterCatalogItemRasterDataset(rasterWsEx, catalogname, itemname));

                rasterLayer.Name = rasterLayer.Name.Substring(0,rasterLayer.Name.LastIndexOf("\\") + 1) + itemname;
                return rasterLayer;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenRasterCatalogItemLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IFeatureDataset OpenFeatureDataset(IWorkspace workspace, string datasetname)
        {
            return OpenFeatureDataset(workspace as IFeatureWorkspace, datasetname);
        }

        public static IFeatureDataset OpenFeatureDataset(IFeatureWorkspace featworkspace, string datasetname)
        {
            try
            {
                if (featworkspace == null || datasetname.Equals(String.Empty))
                {
                    return null;
                }

                return featworkspace.OpenFeatureDataset(datasetname);
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenFeatureDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static bool ConvertDataTableToFeature(ESRI.ArcGIS.Geodatabase.IFeature feature, DataTable datatable)
        {
            try
            {
                if (feature == null || datatable == null)
                {
                    return false;
                }

                IFeatureClass featureClass = feature.Class as IFeatureClass;
                if (featureClass == null)
                {
                    return false;
                }

                ESRI.ArcGIS.Geodatabase.IField field = null;
                DataRow datarow = null;
                int index = -1;
                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    datarow = datatable.Rows[i];

                    index = featureClass.Fields.FindFieldByAliasName(datarow[0].ToString());
                    if (index > -1)
                    {
                        field = featureClass.Fields.get_Field(index);
                        if (field.Type == esriFieldType.esriFieldTypeOID || field.Type == esriFieldType.esriFieldTypeRaster || field.Type == esriFieldType.esriFieldTypeGeometry
                        || field.Type == esriFieldType.esriFieldTypeBlob || field.Name.ToUpper().Contains("SHAPE."))
                        {
                            continue;
                        }

                        if (datarow[1].ToString().Equals(String.Empty))
                        {
                            continue;
                        }

                        feature.set_Value(index, datarow[1]);
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertDataTableToFeature:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static DataTable ConvertFeatureToDataTable(ESRI.ArcGIS.Geodatabase.IFeature feature)
        {
            try
            {
                if (feature == null)
                {
                    return null;
                }

                DataTable datatable = new DataTable("数据表");
                datatable.Columns.Add("FEATUREFIELD");
                datatable.Columns.Add("VALUE");

                DataRow datarow = null;
                IFeatureClass featureClass = feature.Class as IFeatureClass;
                if (featureClass == null)
                {
                    return null;
                }

                ESRI.ArcGIS.Geodatabase.IField field = null;
                for (int i = 0; i < featureClass.Fields.FieldCount; i++)
                {
                    datarow = datatable.NewRow();
                    field = featureClass.Fields.get_Field(i);

                    datarow[0] = field.AliasName;

                    if (field.Type == esriFieldType.esriFieldTypeRaster)
                    {
                        datarow[1] = "Raster";
                    }
                    else if (field.Type == esriFieldType.esriFieldTypeBlob)
                    {
                        datarow[1] = "Blob";
                    }
                    else if (field.Type == esriFieldType.esriFieldTypeGeometry)
                    {
                        datarow[1] = "Geometry";
                    }
                    else
                    {
                        datarow[1] = feature.get_Value(i).ToString();
                    }


                    datatable.Rows.Add(datarow);
                }

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertFeatureToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static DataTable ConvertGeometryToDatatTable(IGeometry geometry)
        {
            try
            {
                if (geometry == null)
                {
                    return null;
                }

                if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    return ConvertPointCollToDataTable(geometry as IPoint);
                }
                else if (geometry.GeometryType == esriGeometryType.esriGeometryPath || geometry.GeometryType == esriGeometryType.esriGeometryPolyline
                       || geometry.GeometryType == esriGeometryType.esriGeometryRing || geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    return ConvertPointCollToDataTable(geometry as IPointCollection);
                }

                return null;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertGeometryToDatatTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static DataTable ConvertPointCollToDataTable(ESRI.ArcGIS.Geometry.IPoint point)
        {
            try
            {
                if (point == null)
                {
                    return null;
                }

                DataTable datatable = new DataTable("数据表");
                datatable.Columns.Add("NO");
                datatable.Columns.Add("X");
                datatable.Columns.Add("Y");
                datatable.Columns.Add("Z");

                DataRow datarow = datatable.NewRow();
                datarow[0] = "1";
                datarow[1] = point.X.ToString();
                datarow[2] = point.Y.ToString();
                datarow[3] = point.Z.ToString();

                datatable.Rows.Add(datarow);

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertPointCollToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static DataTable ConvertPointCollToDataTable(ESRI.ArcGIS.Geometry.IPointCollection pointColl)
        {
            try
            {
                if (pointColl == null)
                {
                    return null;
                }

                DataTable datatable = new DataTable("数据表");
                datatable.Columns.Add("NO");
                datatable.Columns.Add("X");
                datatable.Columns.Add("Y");
                datatable.Columns.Add("Z");

                DataRow datarow = null;
                for (int i = 0; i < pointColl.PointCount; i++)
                {
                    datarow = datatable.NewRow();
                    datarow[0] = String.Format("{0}", i + 1);
                    datarow[1] = pointColl.get_Point(i).X.ToString();
                    datarow[2] = pointColl.get_Point(i).Y.ToString();
                    datarow[3] = pointColl.get_Point(i).Z.ToString();

                    datatable.Rows.Add(datarow);
                }

                return datatable;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertPointCollToDataTable:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IGeometry ConvertDataTableToGeometry(IGeometry geometry,DataTable datatable)
        {
            try
            {
                if (datatable == null)
                {
                    return geometry;
                }

                IPointCollection pointColl = null;
                DataRow datarow = null;
                IPoint point = null;

                if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    point = geometry as IPoint;

                    for (int i = 0; i < datatable.Rows.Count; i++)
                    {
                        datarow = datatable.Rows[i];
                        point.X = Double.Parse(datarow[1].ToString());
                        point.Y = Double.Parse(datarow[2].ToString());
                        point.Z = Double.Parse(datarow[3].ToString());
                    }

                    return point as IGeometry;
                }
                else if (geometry.GeometryType == esriGeometryType.esriGeometryPath || geometry.GeometryType == esriGeometryType.esriGeometryPolyline
                    || geometry.GeometryType == esriGeometryType.esriGeometryRing || geometry.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    pointColl = geometry as IPointCollection;

                    for (int i = 0; i < datatable.Rows.Count; i++)
                    {
                        point = pointColl.get_Point(i);

                        datarow = datatable.Rows[i];
                        point.X = Double.Parse(datarow[1].ToString());
                        point.Y = Double.Parse(datarow[2].ToString());
                        point.Z = Double.Parse(datarow[3].ToString());

                        pointColl.UpdatePoint(i, point);
                    }
                    return pointColl as IGeometry;
                }

                return geometry;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConvertDataTableToGeometry:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return geometry;
            }
        }

        public static IList<object> GetTableFieldUniqueValue(ESRI.ArcGIS.Geodatabase.ITable table, string fieldName)
        {
            try
            {
                IList<object> uniqueValueList = new List<object>();

                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.SubFields = fieldName;

                ESRI.ArcGIS.Geodatabase.ICursor cursor = table.Search(queryFilter, false);
                ESRI.ArcGIS.Geodatabase.IDataStatistics dataStatistics = new ESRI.ArcGIS.Geodatabase.DataStatisticsClass();

                dataStatistics.Field = fieldName;
                dataStatistics.Cursor = cursor;

                System.Collections.IEnumerator enumerator = dataStatistics.UniqueValues;
                enumerator.Reset();

                while (enumerator.MoveNext())
                {
                    uniqueValueList.Add(enumerator.Current);
                }

                return uniqueValueList;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetTableFieldUniqueValue:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 1、        使用ExecuteSQL删除最快，数据库的效率最高。
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static bool DataDelete(IDataset dataset, string whereClause)
        {
            ITransactions transaction = dataset.Workspace as ITransactions;
            try
            {
                if (transaction.InTransaction)
                {
                    return false;
                }

                transaction.StartTransaction();

                string strSQL = string.Empty;
                if (whereClause.Equals(String.Empty))
                {
                    strSQL = String.Format("delete from {0}", dataset.Name);
                }
                else
                {
                    strSQL = String.Format("delete from {0} where {1}", dataset.Name, whereClause);
                }

                dataset.Workspace.ExecuteSQL(strSQL);

                transaction.CommitTransaction();
                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DataDelete:" + ex.Message);
                transaction.AbortTransaction();
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 作用与上面的函数一样，但事务控制交由外部函数来做
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static bool DataDeleteWithoutTransaction(IDataset dataset, string whereClause)
        {
            try
            {
                string strSQL = string.Empty;
                if (whereClause.Equals(String.Empty))
                {
                    strSQL = String.Format("delete from {0}", dataset.Name);
                }
                else
                {
                    strSQL = String.Format("delete from {0} where {1}", dataset.Name, whereClause);
                }

                dataset.Workspace.ExecuteSQL(strSQL);

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.DataDelete:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }
    }
}
