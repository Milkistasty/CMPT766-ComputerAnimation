using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representation of a 3x3 Matrix.
/// </summary>
public class Matrix3x3
{
    Matrix4x4 _matrix;

    /// <summary>
    /// Returns the inverse of this matrix
    /// </summary>
    public Matrix3x3 inverse { get => _computeInverse(); }

    /// <summary>
    /// Returns the transpose of this matrix
    /// </summary>
    public Matrix3x3 transpose { get => _computeTranspose(); }

    /// <summary>
    /// Creates an identity 3x3 Matrix
    /// </summary>
    public Matrix3x3()
    {
        _matrix = Matrix4x4.identity;
    }

    /// <summary>
    /// Constructs a 3x3 Matrix given column 1,2,3
    /// </summary>
    public Matrix3x3(Vector3 col1, Vector3 col2, Vector3 col3)
    {
        _matrix = Matrix4x4.identity;
        _matrix.SetColumn(0, col1);
        _matrix.SetColumn(1, col2);
        _matrix.SetColumn(2, col3);
    }

    /// <summary>
    /// Constructs a 3x3 Matrix given a rotational quaternion
    /// </summary>
    public Matrix3x3(Quaternion q)
    {
        _matrix = Matrix4x4.identity;
        _matrix.SetTRS(Vector3.zero, q, Vector3.one);
    }

    /// Overload the * operator to multiply a matrix with a vector
    public static Vector3 operator *(Matrix3x3 A, Vector3 x)
    {
        return A._matrix.MultiplyPoint3x4(x);
    }

    /// Overload the * operator to multiply two matrices
    public static Matrix3x3 operator *(Matrix3x3 A, Matrix3x3 B)
    {
        Matrix4x4 mat4x4 = A._matrix * B._matrix;
        return new Matrix3x3(
            mat4x4.GetColumn(0),
            mat4x4.GetColumn(1),
            mat4x4.GetColumn(2)
        );
    }

    /// Overload the + operator to add two matrices
    public static Matrix3x3 operator +(Matrix3x3 A, Matrix3x3 B)
    {
        return new Matrix3x3(
            A._matrix.GetColumn(0) + B._matrix.GetColumn(0),
            A._matrix.GetColumn(1) + B._matrix.GetColumn(1),
            A._matrix.GetColumn(2) + B._matrix.GetColumn(2)
        );
    }

    private Matrix3x3 _computeInverse()
    {
        Matrix3x3 inv = new Matrix3x3();
        inv._matrix = _matrix.inverse;
        return inv;
    }

    private Matrix3x3 _computeTranspose()
    {
        Matrix3x3 transpose = new Matrix3x3(
            _matrix.GetRow(0),
            _matrix.GetRow(1),
            _matrix.GetRow(2)
        );
        return transpose;
    }

    public override string ToString()
    {
        return _matrix.ToString();
    }
}
