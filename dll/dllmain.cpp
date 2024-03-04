// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <mkl.h>
#include <mkl_df_defines.h>

#include <iostream>
#include <vector>

enum class ErrorCode { NO, INIT_ERROR, CHECK_ERROR, SOLVE_ERROR, JACOBI_ERROR, GET_ERROR, DELETE_ERROR, RCI_ERROR };

struct SplineDesignInformation
{
    MKL_INT splineOrder;
    MKL_INT boundaryConditionType;
    MKL_INT splineType;
    double* borders;
    double* nonUniformGrid;
    double* valuesNonUniformGrid;
    int nonUniformGridLength;
    int m;
    double* splineCoefficients;
    int residual;

};

// подсчёт значений сплайна на неравномерной сетке
void SplineOnUnevenMesh(MKL_INT* nX, MKL_INT* m, double* valuesOnUniformGrid, double* f, void* userData)
{
    // void * - единственный допустимый вариант передачи остальных параметов
    //SplineDesignInformation params = *((SplineDesignInformation*)userData);
    SplineDesignInformation params = *(static_cast<SplineDesignInformation*>(userData));
    int status = -1;
    DFTaskPtr task;

    // строим равномерный сплайн
    status = dfdNewTask1D(
        &task, params.m, params.borders,
        DF_UNIFORM_PARTITION,
        1, valuesOnUniformGrid,
        DF_NO_HINT);

    if (status != DF_STATUS_OK) throw "Error dfdNewTask1D";

    // записываем коэффициенты в splineCoefficients, DF_NO_HINT - для оптимизации
    status = dfdEditPPSpline1D(task,
        params.splineOrder, params.splineType,
        params.boundaryConditionType, NULL,
        DF_NO_IC, NULL,
        params.splineCoefficients, DF_NO_HINT);

    if (status != DF_STATUS_OK) throw "Error dfdEditPPSpline1D";

    // конструирование сплайна
    status = dfdConstruct1D(task,
        DF_PP_SPLINE,
        DF_METHOD_STD);

    if (status != DF_STATUS_OK) throw "Error dfdConstruct1D";

    // флаг порядка подчёта производных
    int nDorder = 1;
    MKL_INT dOrder[] = { 1 };
    double* splineWithDerivatives = new double[*nX];

    // подсчёт значений функций на неравномерной сетке
    status = dfdInterpolate1D(task,
        DF_INTERP, DF_METHOD_PP,
        params.nonUniformGridLength, params.nonUniformGrid,
        DF_NON_UNIFORM_PARTITION,
        nDorder, dOrder,
        NULL,
        splineWithDerivatives,
        DF_NO_HINT, NULL);

    if (status != DF_STATUS_OK) throw "Error dfdInterpolate1D";


    for (int i = 0; i < params.nonUniformGridLength; ++i) {
        // считаем квадрат невязки
        if (params.residual) {
            f[i] = std::pow((splineWithDerivatives[i] - params.valuesNonUniformGrid[i]), 2);
        }
        else {
            f[i] = splineWithDerivatives[i];
        }

    }
    status = dfDeleteTask(&task);
    if (status != DF_STATUS_OK) std::cerr << "Error dfDeleteTask" << std::endl;

    delete[] splineWithDerivatives;
}

extern "C" _declspec(dllexport)
void SplineInterpolation(
    int numNonUniformNodes,
    double* nonUniformNodes,
    int vectorDimension,
    double* vectorValues,
    int numUniformNodes,
    double* initialApproximation,
    double* splineValues,
    int* stopReason,
    int maxIterations,
    int* actualNumberOfIterations,
    double* smallerNetValues,
    double *smallerNetGrid,
    int smallerNetValuesLength)
{
    MKL_INT splineOrder = DF_PP_CUBIC; // кубический сплайн
    MKL_INT splineType = DF_PP_NATURAL; // простой сплай
    MKL_INT bcType = DF_BC_FREE_END; // типы граничных условий
    double* splineCoefficients = new double[1 * (numUniformNodes - 1) * splineOrder];
    int status = -1;

    MKL_INT maxOuterIterations = maxIterations;
    MKL_INT maxInnerIterations = 100;

    MKL_INT doneIterations = 0;
    double rho = 10;

    const double eps[] = {
        1.0E-12,
        1.0E-12,
        1.0E-12,
        1.0E-12,
        1.0E-12,
        1.0E-12
    };

    double jacobianEps = 1.0E-8;

    double initialResidual = 0;
    double finalResidual = 0;
    MKL_INT terminationReason;
    MKL_INT checkDataInfo[4];
    ErrorCode error = ErrorCode(ErrorCode::NO);

    _TRNSP_HANDLE_t handle = NULL;
    double* residualVector = NULL, * jacobianMatrix = NULL;

    try
    {
        double boundaries[2] = { nonUniformNodes[0], nonUniformNodes[numNonUniformNodes - 1] };

        residualVector = new double[numNonUniformNodes];
        jacobianMatrix = new double[numNonUniformNodes * numUniformNodes];

        MKL_INT ret = dtrnlsp_init(&handle, &numUniformNodes, &numNonUniformNodes, initialApproximation, eps, &maxOuterIterations, &maxInnerIterations, &rho);
        if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::INIT_ERROR));

        ret = dtrnlsp_check(&handle, &numUniformNodes, &numNonUniformNodes, jacobianMatrix, residualVector, eps, checkDataInfo);
        if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::CHECK_ERROR));

        MKL_INT rciRequest = 0;

        SplineDesignInformation SplineDesignInformation;
        SplineDesignInformation.nonUniformGridLength = numNonUniformNodes;
        SplineDesignInformation.nonUniformGrid = nonUniformNodes;
        SplineDesignInformation.valuesNonUniformGrid = vectorValues;
        SplineDesignInformation.m = numUniformNodes;
        SplineDesignInformation.borders = boundaries;
        SplineDesignInformation.splineCoefficients = splineCoefficients;
        SplineDesignInformation.splineOrder = splineOrder;
        SplineDesignInformation.splineType = splineType;
        SplineDesignInformation.boundaryConditionType = bcType;
        SplineDesignInformation.residual = true;

        bool skipSplineConstruction = false;
        while (true)
        {
            if (!skipSplineConstruction) {
                skipSplineConstruction = true;
            }

            ret = dtrnlsp_solve(&handle, residualVector, jacobianMatrix, &rciRequest);
            if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::SOLVE_ERROR));

            if (rciRequest == 0) continue;
            else if (rciRequest == 1) SplineOnUnevenMesh(&numNonUniformNodes, &numUniformNodes, initialApproximation, residualVector, static_cast<void*>(&SplineDesignInformation));
            else if (rciRequest == 2)
            {
                ret = djacobix(SplineOnUnevenMesh, &numUniformNodes, &numNonUniformNodes, jacobianMatrix, initialApproximation, &jacobianEps, static_cast<void*>(&SplineDesignInformation));
                if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::JACOBI_ERROR));
            }
            else if (rciRequest >= -6 && rciRequest <= -1) break;
            else throw (ErrorCode(ErrorCode::RCI_ERROR));
        }

        ret = dtrnlsp_get(&handle, &doneIterations, &terminationReason, &initialResidual, &finalResidual);
        if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::GET_ERROR));

        ret = dtrnlsp_delete(&handle);
        if (ret != TR_SUCCESS) throw (ErrorCode(ErrorCode::DELETE_ERROR));

        double* values = new double[numNonUniformNodes];
        SplineDesignInformation.residual = false;
        SplineOnUnevenMesh(&numNonUniformNodes, 
            &numUniformNodes, 
            initialApproximation, 
            values, 
            static_cast<void*>(&SplineDesignInformation));

        for (int i = 0; i < numNonUniformNodes; ++i)
        {
            splineValues[i] = values[i];
        }

        SplineDesignInformation.nonUniformGridLength = smallerNetValuesLength;
        SplineDesignInformation.nonUniformGrid = smallerNetGrid;
        SplineOnUnevenMesh(&smallerNetValuesLength, 
            &numUniformNodes, 
            initialApproximation, 
            smallerNetValues, 
            static_cast<void*>(&SplineDesignInformation));

        *stopReason = terminationReason;
        *actualNumberOfIterations = doneIterations;

        delete[] values;
    }
    catch (ErrorCode _error) { error = _error; }
    catch (const char* str)
    {
        std::cerr << std::string(str) << std::endl;
        std::cout << std::string(str) << std::endl;
    }

    if (residualVector != NULL) delete[] residualVector;
    if (jacobianMatrix != NULL) delete[] jacobianMatrix;

    delete[] splineCoefficients;
}