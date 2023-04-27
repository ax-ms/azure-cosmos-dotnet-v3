//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

using System;
using System.Collections.Generic;

using static Microsoft.Data.Encryption.Resources.Strings;

namespace Microsoft.Data.Encryption.Cryptography.Serializers
{
    /// <summary>
    /// Provides methods for getting serializer implementations, such as by type and ID.
    /// </summary>
    internal class SqlSerializerFactory : SerializerFactory
    {
        private readonly Dictionary<Type, ISerializer> serializerByType = new Dictionary<Type, ISerializer>();

        private readonly Dictionary<string, ISerializer> serializerByIdentifier = new Dictionary<string, ISerializer>();

        private static readonly Dictionary<Type, Func<int, byte, byte, int, ISerializer>> createSerializerByType = new Dictionary<Type, Func<int, byte, byte, int, ISerializer>>()
        {
            [typeof(SqlBitSerializer)] = (size, precision, scale, codepage) => new SqlBitSerializer(),
            [typeof(SqlNullableBitSerializer)] = (size, precision, scale, codepage) => new SqlNullableBitSerializer(),
            [typeof(SqlTinyIntSerializer)] = (size, precision, scale, codepage) => new SqlTinyIntSerializer(),
            [typeof(SqlNullableTinyIntSerializer)] = (size, precision, scale, codepage) => new SqlNullableTinyIntSerializer(),
            [typeof(SqlVarBinarySerializer)] = (size, precision, scale, codepage) => new SqlVarBinarySerializer(size),
            [typeof(SqlDateTime2Serializer)] = (size, precision, scale, codepage) => new SqlDateTime2Serializer(precision),
            [typeof(SqlNullableDateTime2Serializer)] = (size, precision, scale, codepage) => new SqlNullableDateTime2Serializer(precision),
            [typeof(SqlDateTimeOffsetSerializer)] = (size, precision, scale, codepage) => new SqlDateTimeOffsetSerializer(scale),
            [typeof(SqlNullableDateTimeOffsetSerializer)] = (size, precision, scale, codepage) => new SqlNullableDateTimeOffsetSerializer(scale),
            [typeof(SqlDecimalSerializer)] = (size, precision, scale, codepage) => new SqlDecimalSerializer(precision, scale),
            [typeof(SqlNullableDecimalSerializer)] = (size, precision, scale, codepage) => new SqlNullableDecimalSerializer(precision, scale),
            [typeof(SqlFloatSerializer)] = (size, precision, scale, codepage) => new SqlFloatSerializer(),
            [typeof(SqlNullableFloatSerializer)] = (size, precision, scale, codepage) => new SqlNullableFloatSerializer(),
            [typeof(SqlRealSerializer)] = (size, precision, scale, codepage) => new SqlRealSerializer(),
            [typeof(SqlNullableRealSerializer)] = (size, precision, scale, codepage) => new SqlNullableRealSerializer(),
            [typeof(SqlUniqueIdentifierSerializer)] = (size, precision, scale, codepage) => new SqlUniqueIdentifierSerializer(),
            [typeof(SqlNullableUniqueIdentifierSerializer)] = (size, precision, scale, codepage) => new SqlNullableUniqueIdentifierSerializer(),
            [typeof(SqlIntSerializer)] = (size, precision, scale, codepage) => new SqlIntSerializer(),
            [typeof(SqlNullableIntSerializer)] = (size, precision, scale, codepage) => new SqlNullableIntSerializer(),
            [typeof(SqlBigIntSerializer)] = (size, precision, scale, codepage) => new SqlBigIntSerializer(),
            [typeof(SqlNullableBigIntSerializer)] = (size, precision, scale, codepage) => new SqlNullableBigIntSerializer(),
            [typeof(SqlSmallIntSerializer)] = (size, precision, scale, codepage) => new SqlSmallIntSerializer(),
            [typeof(SqlNullableSmallIntSerializer)] = (size, precision, scale, codepage) => new SqlNullableSmallIntSerializer(),
            [typeof(SqlNVarCharSerializer)] = (size, precision, scale, codepage) => new SqlNVarCharSerializer(size),
            [typeof(SqlTimeSerializer)] = (size, precision, scale, codepage) => new SqlTimeSerializer(scale),
            [typeof(SqlNullableTimeSerializer)] = (size, precision, scale, codepage) => new SqlNullableTimeSerializer(scale),
            [typeof(SqlBinarySerializer)] = (size, precision, scale, codepage) => new SqlBinarySerializer(size),
            [typeof(SqlDateSerializer)] = (size, precision, scale, codepage) => new SqlDateSerializer(),
            [typeof(SqlNullableDateSerializer)] = (size, precision, scale, codepage) => new SqlNullableDateSerializer(),
            [typeof(SqlDateTimeSerializer)] = (size, precision, scale, codepage) => new SqlDateTimeSerializer(),
            [typeof(SqlNullableDateTimeSerializer)] = (size, precision, scale, codepage) => new SqlNullableDateTimeSerializer(),
            [typeof(SqlSmallDateTimeSerializer)] = (size, precision, scale, codepage) => new SqlSmallDateTimeSerializer(),
            [typeof(SqlNullableSmallDateTimeSerializer)] = (size, precision, scale, codepage) => new SqlNullableSmallDateTimeSerializer(),
            [typeof(SqlMoneySerializer)] = (size, precision, scale, codepage) => new SqlMoneySerializer(),
            [typeof(SqlNullableMoneySerializer)] = (size, precision, scale, codepage) => new SqlNullableMoneySerializer(),
            [typeof(SqlNumericSerializer)] = (size, precision, scale, codepage) => new SqlNumericSerializer(precision, scale),
            [typeof(SqlNullableNumericSerializer)] = (size, precision, scale, codepage) => new SqlNullableNumericSerializer(precision, scale),
            [typeof(SqlSmallMoneySerializer)] = (size, precision, scale, codepage) => new SqlSmallMoneySerializer(),
            [typeof(SqlNullableSmallMoneySerializer)] = (size, precision, scale, codepage) => new SqlNullableSmallMoneySerializer(),
            [typeof(SqlNCharSerializer)] = (size, precision, scale, codepage) => new SqlNCharSerializer(size),
            [typeof(SqlCharSerializer)] = (size, precision, scale, codepage) => new SqlCharSerializer(size, codepage),
            [typeof(SqlVarCharSerializer)] = (size, precision, scale, codepage) => new SqlVarCharSerializer(size, codepage),
        };

        private static readonly LocalCache<Tuple<Type, int, byte, byte, int>, ISerializer> serializerCache
            = new LocalCache<Tuple<Type, int, byte, byte, int>, ISerializer>();

        /// <summary>
        /// Returns a cached instance of the <see cref="ISerializer"/> or, if not present, creates a new one.
        /// </summary>
        /// <param name="serializerType">The type of serializer to get or create.</param>
        /// <param name="size">The maximum size of the data.</param>
        /// <param name="precision">The maximum number of digits.</param>
        /// <param name="scale">The number of decimal places.</param>
        /// <param name="codepage">The code page character encoding.</param>
        /// <returns>An  <see cref="ISerializer"/> object.</returns>
        public static ISerializer GetOrCreate(Type serializerType, int size = 1, byte precision = 1, byte scale = 1, int codepage = 1)
        {
            serializerType.ValidateNotNull(nameof(serializerType));

            return serializerCache.GetOrCreate(
                key: Tuple.Create(serializerType, size, precision, scale, codepage),
                createItem: () => createSerializerByType[serializerType].Invoke(size, precision, scale, codepage)
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSerializerFactory"/> class.
        /// </summary>
        public SqlSerializerFactory()
        {
            Initialize();
        }

        /// <summary>
        /// Returns a default instance of the <see cref="SqlSerializerFactory"/> class.
        /// </summary>
        public static SqlSerializerFactory Default { get; } = new SqlSerializerFactory();

        /// <summary>
        /// Gets a registered serializer by its Identifier Property.
        /// </summary>
        /// <param name="identifier">The identifier uniquely identifies a particular Serializer implementation.</param>
        /// <returns>The ISerializer implementation</returns>
        public override ISerializer GetSerializer(string identifier)
        {
            identifier.ValidateNotNull(nameof(identifier));

            if (serializerByIdentifier.ContainsKey(identifier))
            {
                return serializerByIdentifier[identifier];
            }

            return null;
        }

        /// <summary>
        /// Gets a default registered serializer for the type.
        /// </summary>
        /// <typeparam name="T">The data type to be serialized.</typeparam>
        /// <returns>A default registered serializer for the type.</returns>
        public override Serializer<T> GetDefaultSerializer<T>()
        {
            if (serializerByType.ContainsKey(typeof(T)))
            {
                return (Serializer<T>)serializerByType[typeof(T)];
            }

            throw new MicrosoftDataEncryptionException(DefaultAESerializerNotFound.Format(typeof(T).Name, nameof(RegisterSerializer)));
        }

        /// <summary>
        /// Registers a custom serializer.
        /// </summary>
        /// <param name="type">The data type on which the Serializer operates.</param>
        /// <param name="serializer">The Serializer to register.</param>
        /// <param name="overrideDefault">Determines whether to override an existing default serializer for the same type.</param>
        public override void RegisterSerializer(Type type, ISerializer serializer, bool overrideDefault = false)
        {
            type.ValidateNotNull(nameof(type));
            serializer.ValidateNotNull(nameof(serializer));

            serializerByIdentifier[serializer.Identifier] = serializer;

            if (overrideDefault || !HasDefaultSqlSerializer(type))
            {
                serializerByType[type] = serializer;
            }
        }

        private bool HasDefaultSerializer(Type type)
        {
            return serializerByType.ContainsKey(type);
        }

        private bool HasDefaultSqlSerializer(Type type)
        {
            return serializerByType.ContainsKey(type);
        }

        private void Initialize()
        {
            RegisterDefaultSqlSerializers();
            RegisterNonDefaultSqlSerializers();
        }

        private void RegisterDefaultSqlSerializers()
        {
            RegisterSerializer(typeof(bool), new SqlBitSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(bool?), new SqlNullableBitSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(byte), new SqlTinyIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(byte?), new SqlNullableTinyIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(byte[]), new SqlVarBinarySerializer(), overrideDefault: true);
            RegisterSerializer(typeof(DateTime), new SqlDateTime2Serializer(), overrideDefault: true);
            RegisterSerializer(typeof(DateTime?), new SqlNullableDateTime2Serializer(), overrideDefault: true);
            RegisterSerializer(typeof(DateTimeOffset), new SqlDateTimeOffsetSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(DateTimeOffset?), new SqlNullableDateTimeOffsetSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(decimal), new SqlDecimalSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(decimal?), new SqlNullableDecimalSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(double), new SqlFloatSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(double?), new SqlNullableFloatSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(float), new SqlRealSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(float?), new SqlNullableRealSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(Guid), new SqlUniqueIdentifierSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(Guid?), new SqlNullableUniqueIdentifierSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(int), new SqlIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(int?), new SqlNullableIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(long), new SqlBigIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(long?), new SqlNullableBigIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(short), new SqlSmallIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(short?), new SqlNullableSmallIntSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(string), new SqlNVarCharSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(TimeSpan), new SqlTimeSerializer(), overrideDefault: true);
            RegisterSerializer(typeof(TimeSpan?), new SqlNullableTimeSerializer(), overrideDefault: true);
        }

        private void RegisterNonDefaultSqlSerializers()
        {
            RegisterSerializer(typeof(byte[]), new SqlBinarySerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime), new SqlDateSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime?), new SqlNullableDateSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime), new SqlDateTimeSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime?), new SqlNullableDateTimeSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime), new SqlSmallDateTimeSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(DateTime), new SqlNullableSmallDateTimeSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal), new SqlMoneySerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal?), new SqlNullableMoneySerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal), new SqlNumericSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal?), new SqlNullableNumericSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal), new SqlSmallMoneySerializer(), overrideDefault: false);
            RegisterSerializer(typeof(decimal?), new SqlNullableSmallMoneySerializer(), overrideDefault: false);
            RegisterSerializer(typeof(string), new SqlNCharSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(string), new SqlCharSerializer(), overrideDefault: false);
            RegisterSerializer(typeof(string), new SqlVarCharSerializer(), overrideDefault: false);
        }
    }
}