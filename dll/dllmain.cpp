// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <mkl.h>
#include <mkl_df_defines.h>

#include <iostream>
#include <vector>

enum class ErrorEnum { NO, INIT, CHECK, SOLVE, JACOBI, GET, MYDELETE, RCI };

struct Help_info
{
	int non_uniform_grid_len;
	double* non_uniform_grid;
	double* values_non_uniform_grid;
	int m;
	double* borders;
	double* scoeff;
	MKL_INT s_order;
	MKL_INT s_type;
	MKL_INT bc_type;
};

void
Function(MKL_INT* nX, MKL_INT* m, double* values_on_uniform_grid, double* f, void* user_data)
{
	Help_info info = *((Help_info*)user_data);
	int status = -1;
	DFTaskPtr task;
	int m_inside = info.m;
	double* borders = info.borders;
	MKL_INT s_order = info.s_order;
	MKL_INT s_type = info.s_type;
	MKL_INT bc_type = info.bc_type;
	double* scoeff = info.scoeff;
	status = dfdNewTask1D(&task,
		m_inside, borders,
		DF_UNIFORM_PARTITION,
		1, values_on_uniform_grid,
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
	int nDorder = 1;
	MKL_INT dorder[] = { 1 };
	int nnon_zero_ders = 1;
	double* spline_with_derivatives = new double[*nX * nnon_zero_ders];
	status = dfdInterpolate1D(task,
		DF_INTERP, DF_METHOD_PP,
		info.non_uniform_grid_len, info.non_uniform_grid,
		DF_NON_UNIFORM_PARTITION,
		nDorder, dorder,
		NULL,
		spline_with_derivatives, DF_NO_HINT, NULL);
	if (status != DF_STATUS_OK) throw "Couldn't interpolate";
	double* x = new double[info.non_uniform_grid_len];
	for (int i = 0; i < info.non_uniform_grid_len; ++i)
	{
		x[i] = spline_with_derivatives[i * nnon_zero_ders];
	}
	// x  хранит значения на неравномерной сетке
	for (int i = 0; i < info.non_uniform_grid_len; ++i) f[i] = std::pow((x[i] - info.values_non_uniform_grid[i]), 2);
	delete[] x;
	status = dfDeleteTask(&task);
	if (status != DF_STATUS_OK) std::cerr << "shit" << std::endl;
	delete[] spline_with_derivatives;
}

void
calculate_spline(MKL_INT *nX, double* storage, MKL_INT *m, double* values_on_uniform_grid, void* user_data)
{
	Help_info info = *((Help_info*)user_data);
	int status = -1;
	DFTaskPtr task;
	int m_inside = info.m;
	double* borders = info.borders;
	MKL_INT s_order = info.s_order;
	MKL_INT s_type = info.s_type;
	MKL_INT bc_type = info.bc_type;
	double* scoeff = info.scoeff;
	status = dfdNewTask1D(&task,
		m_inside, borders,
		DF_UNIFORM_PARTITION,
		1, values_on_uniform_grid,
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
	int nDorder = 1;
	MKL_INT dorder[] = { 1 };
	int nnon_zero_ders = 1;
	double* spline_with_derivatives = new double[*nX * nnon_zero_ders];
	status = dfdInterpolate1D(task,
		DF_INTERP, DF_METHOD_PP,
		info.non_uniform_grid_len, info.non_uniform_grid,
		DF_NON_UNIFORM_PARTITION,
		nDorder, dorder,
		NULL,
		spline_with_derivatives, DF_NO_HINT, NULL);
	if (status != DF_STATUS_OK) throw "Couldn't interpolate";
	for (int i = 0; i < info.non_uniform_grid_len; ++i)
	{
		storage[i] = spline_with_derivatives[i * nnon_zero_ders];
	}
	// x  хранит значения на неравномерной сетке
	status = dfDeleteTask(&task);
	if (status != DF_STATUS_OK) std::cerr << "shit" << std::endl;
	delete[] spline_with_derivatives;
}

extern "C" _declspec(dllexport)
void spline(
	int nX,									// Число узлов неравномерной сетки
	double* X,								// Массив узлов неравномерной сетки
	int nY,									// размерность векторной функции(в нашем случае 1)
	double* Y,								// массив значений функции в узлах сетки
	int m,									// Количество узлов равномерной сетки
	double* values_on_uniform_grid,			// Начальная аппроксимация
	double* splineValues,					// Массив для возврата значений функции и ее производных в узлах неравномерной сетки
	int* stop_reason,						// Переменная для возврата кода причины остановки итерационного процесса
	int max_iterations,
	int *actual_number_of_iterations)						
{
	MKL_INT s_order = DF_PP_CUBIC;			// Степень кубического сплайна
	MKL_INT s_type = DF_PP_NATURAL;			// тип сплайна
	MKL_INT bc_type = DF_BC_FREE_END;		// тип граничных условий - вторая производная на концах равна нулю
	double* scoeff = new double[1 * (m - 1) * s_order];
	int status = -1;

	MKL_INT niter1 = max_iterations;
	MKL_INT niter2 = 100;

	MKL_INT ndone_iter = 0;
	double rs = 10;

	const double eps[] = {
		1.0E-12 ,		// размер доверительной области
		1.0E-12 ,		// норма целевой функции
		1.0E-12 ,		// норма строк матрицы Якоби 
		1.0E-12 ,		// точность пробного шага
		1.0E-12 ,		// разность нормы целевой функции 
						// и погрешности аппроксимации функции
		1.0E-12			// точность вычисления пробного шага
	};
	double jac_eps = 1.0E-8; // точность вычиисления матрицы Якоби

	double res_initial = 0; // начальное значение невязки 
	double res_final = 0; // финальное значение невязки 
	MKL_INT stop_criteria; // причина остановки итераций 
	MKL_INT check_data_info[4]; // результат проверки корректности данных 
	ErrorEnum error = ErrorEnum(ErrorEnum::NO); // информация об ошибке

	_TRNSP_HANDLE_t handle = NULL;
	double* fvec = NULL, * fjac = NULL;
	error = ErrorEnum(ErrorEnum::NO);

	try
	{
		// Creating task
		double borders[2]{ X[0], X[nX - 1] };
		
		fvec = new double[nX];
		fjac = new double[nX * m];

		// Инициализация задачи
		MKL_INT ret = dtrnlsp_init(&handle, &m, &nX, values_on_uniform_grid, eps, &niter1, &niter2, &rs);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::INIT));

		// Проверка корректности входных данных
		ret = dtrnlsp_check(&handle, &m, &nX, fjac, fvec, eps, check_data_info);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::CHECK));

		MKL_INT RCI_Request = 0;

		Help_info help;
		help.non_uniform_grid_len = nX;
		help.non_uniform_grid = X;
		help.values_non_uniform_grid = Y;
		help.m = m;
		help.borders = borders;
		help.scoeff = scoeff;
		help.s_order = s_order;
		help.s_type = s_type;
		help.bc_type = bc_type;
		// Итерационный процесс
		bool skip_spline_construct = false;
		while (true)
		{
			if (!skip_spline_construct) {
				skip_spline_construct = true;
			}
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
			if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::SOLVE));
			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1) Function(&nX, &m, values_on_uniform_grid, fvec, static_cast<void*>(&help));
			else if (RCI_Request == 2)
			{
				ret = djacobix(Function, &m, &nX, fjac, values_on_uniform_grid, &jac_eps, static_cast<void*>(&help));
				if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::JACOBI));
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1) break;
			else throw (ErrorEnum(ErrorEnum::RCI));
		}
		// Завершение итерационного процесса
		ret = dtrnlsp_get(&handle, &ndone_iter, &stop_criteria, &res_initial, &res_final);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::GET));
		// Освобождение ресурсов
		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::MYDELETE));
		// Сохранение результатов
		// Вычисление значений сплайна на неравномерной сетке
		double* storage = new double[nX];
		calculate_spline(&nX, storage, &m, values_on_uniform_grid, static_cast<void*>(&help));
		for (int i = 0; i < nX; ++i)
		{
			splineValues[i] = storage[i];
		}
		*stop_reason = stop_criteria;
		*actual_number_of_iterations = ndone_iter;
		delete[] storage;
	}
	catch (ErrorEnum _error) { error = _error; }
	catch (const char* str)
	{
		std::cerr << std::string(str) << std::endl;
		std::cout << std::string(str) << std::endl;
	}

	// Освобождение памяти
	if (fvec != NULL) delete[] fvec;
	if (fjac != NULL) delete[] fjac;

	delete[] scoeff;
}