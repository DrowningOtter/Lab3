// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <mkl.h>
#include <mkl_df_defines.h>
#include <iostream>

//BOOL APIENTRY DllMain( HMODULE hModule,
//                       DWORD  ul_reason_for_call,
//                       LPVOID lpReserved
//                     )
//{
//    switch (ul_reason_for_call)
//    {
//    case DLL_PROCESS_ATTACH:
//    case DLL_THREAD_ATTACH:
//    case DLL_THREAD_DETACH:
//    case DLL_PROCESS_DETACH:
//        break;
//    }
//    return TRUE;
//}

//extern "C"  _declspec(dllexport)
//void spline_inter(MKL_INT sites_amount, float* first_NU_grid, float* values_first_NU_grid, float* der,
//    float* second_NU_grid, int second_sites, float* values_second_NU_grid, float* scoeffhint, int& ret,
//    float* integrals, float* left_lim, float* right_lim)
//{
//    try
//    {
//        DFTaskPtr descr;
//        //float* scoeff = new float[2 * (sites_amount - 1) * DF_PP_CUBIC];
//        int ret_value = dfsNewTask1D(&descr, sites_amount, first_NU_grid, DF_NON_UNIFORM_PARTITION,
//            2, values_first_NU_grid, DF_MATRIX_STORAGE_ROWS);
//        ret_value = dfsEditPPSpline1D(descr, DF_PP_CUBIC, DF_PP_NATURAL,
//            DF_BC_1ST_LEFT_DER | DF_BC_1ST_RIGHT_DER, der, DF_NO_IC, NULL, scoeffhint, DF_NO_HINT);
//        if (ret_value != DF_STATUS_OK)
//        {
//            ret = ret_value;
//            return;
//        }
//        ret_value = dfsConstruct1D(descr, DF_PP_SPLINE, DF_METHOD_STD);
//        if (ret_value != DF_STATUS_OK)
//        {
//            ret = ret_value;
//            return;
//        }
//        int dorder[3]{ 1, 1, 1 };
//        ret_value = dfsInterpolate1D(descr, DF_INTERP, DF_METHOD_PP, second_sites,
//            second_NU_grid, DF_NON_UNIFORM_PARTITION, 3, dorder,
//            NULL, values_second_NU_grid, DF_MATRIX_STORAGE_ROWS, NULL);
//        if (ret_value != DF_STATUS_OK)
//        {
//            ret = ret_value;
//            return;
//        }
//        ret_value = dfsIntegrate1D(descr, DF_METHOD_PP, 1, left_lim, DF_NON_UNIFORM_PARTITION,
//            right_lim, DF_NON_UNIFORM_PARTITION, NULL, NULL, integrals,
//            DF_MATRIX_STORAGE_ROWS);
//        if (ret_value != DF_STATUS_OK)
//        {
//            ret = ret_value;
//            return;
//        }
//        ret_value = dfDeleteTask(&descr);
//        ret = ret_value;
//    }
//    catch (...)
//    {
//        ret = -1;
//    }
//}

extern "C" _declspec(dllexport)
void spline()
{
	std::cout << "I am fucking alive" << std::endl;
}