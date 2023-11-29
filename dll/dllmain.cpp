// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <mkl.h>
#include <mkl_df_defines.h>

#include <iostream>
#include <vector>

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

double
func1(const double val)
{
	return 2 * val;
}

std::vector<double>
linspace(const double left_border, const double right_border, int num_of_dots)
{
	std::vector<double> net(num_of_dots);
	double step = (right_border - left_border) / (num_of_dots - 1);
	for (int i = 0; i < num_of_dots; ++i)
	{
		net[i] = left_border + step * i;
	}
	return net;
}

std::vector<std::vector<double>>
calc_values_by_net(const double left_border, const double right_border, int num_of_dots, double(*func)(const double))
{
	std::vector<double> net = linspace(left_border, right_border, num_of_dots);
	std::vector<double> values(num_of_dots);
	for (int i = 0; i < num_of_dots; ++i)
	{
		values[i] = func(net[i]);
	}
	return { net, values };
}

extern "C" _declspec(dllexport)
void spline(
	int nX, 
	double *X,
	int nY,
	double *Y,
	int m,
	double *splineValues)
{
	MKL_INT s_order = DF_PP_CUBIC; // Степень кубического сплайна
	MKL_INT s_type = DF_PP_NATURAL; // тип сплайна
	MKL_INT bc_type = DF_BC_FREE_END; // тип граничных условий - вторая производная на концах равна нулю
	double* scoeff = new double[nY * (nX - 1) * s_order];
	try
	{
		DFTaskPtr task;
		int status = -1;
		// Creating task
		double borders[2]{X[0], X[nX - 1]};
		std::vector<std::vector<double>> new_with_values = calc_values_by_net(borders[0], borders[1], m, func1);
		double *initial_approximation = new_with_values[1].data();
		status = dfdNewTask1D(&task,
			m, borders,
			DF_UNIFORM_PARTITION,
			nY, initial_approximation,
			DF_NO_HINT);
		if (status != DF_STATUS_OK) throw "Couldn't create task";
		status = dfdEditPPSpline1D(task,
			s_order, s_type,
			bc_type, NULL,
			DF_NO_IC, NULL,
			scoeff, DF_NO_HINT);
		if (status != DF_STATUS_OK) throw "Couldn't configure task";
		status = dfdConstruct1D(task,
			DF_PP_SPLINE,
			DF_METHOD_STD);
		if (status != DF_STATUS_OK) throw "Couldn't construct spline";
		// Подсчет значений сплайна на неравномерной сетки для дальнейшей оптимизации
		int nDorder = 3;
		MKL_INT dorder[] = {1, 0, 1};
		status = dfdInterpolate1D(task,
			DF_INTERP, DF_METHOD_PP,
			nX, X,
			DF_NON_UNIFORM_PARTITION,
			nDorder, dorder,
			NULL,
			splineValues, DF_NO_HINT, NULL);
		if (status != DF_STATUS_OK) throw "Couldn't interpolate";
	}
	catch (const char* str)
	{
		std::cerr << std::string(str) << std::endl;
	}
	// Проверяем, что вторая производная на концах равна нулю
	int not_zero_amount_in_dorder = 2;
	if (splineValues[0 * not_zero_amount_in_dorder + 1] != 0 || splineValues[(nX - 1) * not_zero_amount_in_dorder + 1] != 0)
	{
		std::cout << "Derivative on the end doesn't equal to zero" << std::endl;
	}

	MKL_INT status = -1;
	_TRNSP_HANLDE_t optim_task;
	status = dtrnlsp_init();

	std::cout << std::endl;
	delete[] scoeff;
}