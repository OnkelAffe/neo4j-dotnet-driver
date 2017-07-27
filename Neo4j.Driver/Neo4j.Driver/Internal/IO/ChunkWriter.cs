﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.IO
{
    internal class ChunkWriter: IChunkWriter
    {
        private readonly int _chunkSize;
        private readonly Stream _downStream;
        private readonly MemoryStream _chunkStream;
        private readonly ILogger _logger;

        private readonly byte[] _buffer = new byte[8 * 1024];

        public ChunkWriter(Stream downStream)
            : this(downStream, null)
        {
            
        }

        public ChunkWriter(Stream downStream, int chunkSize)
            : this(downStream, null, chunkSize)
        {

        }

        public ChunkWriter(Stream downStream, ILogger logger)
            : this(downStream, logger, Constants.MaxChunkSize)
        {

        }

        public ChunkWriter(Stream downStream, ILogger logger, int chunkSize)
        {
            Throw.ArgumentNullException.IfNull(downStream, nameof(downStream));
            Throw.ArgumentOutOfRangeException.IfValueLessThan(chunkSize, Constants.MinChunkSize, nameof(chunkSize));
            Throw.ArgumentOutOfRangeException.IfValueGreaterThan(chunkSize, Constants.MaxChunkSize, nameof(chunkSize));

            _logger = logger;
            _chunkSize = chunkSize;
            _downStream = downStream;
            _chunkStream = new MemoryStream();
        }

        public void WriteChunk(byte[] buffer, int offset, int count)
        {
            if (buffer.Length == 0 || count == 0)
            {
                byte[] chunkSize =
                    PackStreamBitConverter.GetBytes((ushort)count);

                _chunkStream.Write(chunkSize, 0, chunkSize.Length);
                _chunkStream.Write(buffer, offset, count);
            }
            else
            {
                var leftToChunk = count;
                var thisChunkIndex = offset;

                while (leftToChunk > 0)
                {
                    var thisChunkSize = (int)Math.Min(leftToChunk, _chunkSize);

                    byte[] chunkSize =
                        PackStreamBitConverter.GetBytes((ushort)thisChunkSize);

                    _chunkStream.Write(chunkSize, 0, chunkSize.Length);
                    _chunkStream.Write(buffer, thisChunkIndex, thisChunkSize);

                    thisChunkIndex += thisChunkSize;
                    leftToChunk -= thisChunkSize;
                }
            }
        }

        public void Flush()
        {
            if (_logger != null)
            {
                byte[] buffer = _chunkStream.ToArray();

                _logger?.Trace("C: ", buffer, 0, buffer.Length);
            }

            _chunkStream.Position = 0;
            _chunkStream.CopyTo(_downStream);

            _chunkStream.Position = 0;
            _chunkStream.SetLength(0);
        }

        public Task FlushAsync()
        {
            if (_logger != null)
            {
                byte[] buffer = _chunkStream.ToArray();

                _logger?.Trace("C: ", buffer, 0, buffer.Length);
            }

            _chunkStream.Position = 0;

            return
                _chunkStream.CopyToAsync(_downStream)
                    .ContinueWith(t =>
                    {
                        _chunkStream.Position = 0;
                        _chunkStream.SetLength(0);

                        return Task.CompletedTask;
                    }).Unwrap();
        }

    }
}
