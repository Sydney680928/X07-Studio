﻿using Accessibility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x07studio.Classes
{
    internal class SerialManager
    {
        private static SerialManager _Default = new();
        private SerialPort? _SerialPort;
        private bool _RequestCancelGetProgram;
        
        public static SerialManager Default => _Default;

        public bool IsOpen => _SerialPort?.IsOpen ?? false;

        public SerialManager()
        {
            
        }

        public bool Open(string portName, int baudRate)
        {
            if (_SerialPort != null)
            {
                _SerialPort.Close();
                _SerialPort = null;
            }

            try
            {
                _SerialPort = new SerialPort()
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    Parity = Parity.None,
                    StopBits = StopBits.Two,
                    DataBits = 8,
                    Handshake = Handshake.RequestToSend,
                    DiscardNull = false,
                    //Encoding = Encoding.ASCII,
                    Encoding = Encoding.GetEncoding(28591),
                    ReadBufferSize = 8196,
                    WriteBufferSize = 16,
                    ReadTimeout = 5000,
                    WriteTimeout = 2000
                };


                _SerialPort.Open();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Close()
        {           
            try
            {
                if (_SerialPort != null && _SerialPort.IsOpen) _SerialPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<GetProgramResponse> GetProgramAsync(int timeout = 5000)
        {
            _RequestCancelGetProgram = false;

            var tcs = new TaskCompletionSource<GetProgramResponse>();

            Task.Run(() => 
            {
                var r = ScanInBuffer(timeout);
                tcs.SetResult(r);   
            });

            return tcs.Task;
        }

        public void RequestCancelGetProgram() => _RequestCancelGetProgram = true;

        public ResponseStatus SendProgram(byte[] program)
        {
            try
            {
                if (_SerialPort != null && _SerialPort.IsOpen)
                {
                    _SerialPort.Write(program, 0, program.Length);
                    return ResponseStatus.Success;
                }
                else
                {
                    return ResponseStatus.PortClosed;
                }
            }
            catch (Exception ex)
            {
                return ResponseStatus.Exception;
            }

        }

        public bool SendCommand(string command)
        {
            if (!string.IsNullOrEmpty(command) && _SerialPort != null && _SerialPort.IsOpen)
            {
                var bytes = Encoding.Unicode.GetBytes(command);
                var b2 = new byte[bytes.Length / 2];
                int i, j;
                for (i = 0, j = 0; i < bytes.Length; i += 2, j++) b2[j] = bytes[i];

                try
                {
                    _SerialPort.Write(b2, 0, b2.Length);
                    _SerialPort.Write("\r");
                    return true;
                }
                catch (Exception ex)
                {

                }
            }

            return false;
        }

        public Task<byte[]> GetDumpAsync(string portname, int baudrate,UInt16 address, UInt16 length, ProgressBar? progress = null)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            Task.Run(() =>
            {
                if (Open(portname, 4800))
                {
                    var command = $"D:{address:X4},{length:X4}";

                    if (_SerialPort != null && SendCommand(command))
                    {
                        if (progress != null && progress.IsHandleCreated)
                        {
                            progress.Invoke(() =>
                            {
                                progress.Value = 0;
                                progress.Minimum = 0;
                                progress.Maximum = length;
                            });
                        }

                        try
                        {
                            var bytes = new byte[length];
                            var index = 0;

                            while (index < length)
                            {
                                bytes[index] = (byte)_SerialPort.ReadByte();
                                index += 1;

                                if (progress != null && progress.IsHandleCreated && index % 50 == 0)
                                {
                                    progress.Invoke(() =>
                                    {
                                        progress.Value = index;
                                    });
                                }
                            }

                            if (progress != null)
                            {
                                progress.Invoke(() =>
                                {
                                    progress.Value = progress.Maximum;
                                });
                            }

                            tcs.SetResult(bytes);
                        }
                        catch (TimeoutException ex)
                        {
                            // Pas de réponse

                            tcs.SetResult([]);
                        }
                        catch (Exception ex)
                        {
                            // Erreur autre que timeout

                            tcs.SetResult([]);
                        }
                        finally
                        {
                            if (progress != null && progress.IsHandleCreated)
                            {
                                Thread.Sleep(1000);

                                progress.Invoke(() =>
                                {
                                    progress.Value = 0;
                                    progress.Minimum = 0;
                                });
                            }
                        }
                    }
                }
            });

            return tcs.Task;    
        }

        private GetProgramResponse ScanInBuffer(int timeout)
        {
            if (_SerialPort == null || !_SerialPort.IsOpen)
            {
                // Le port série est fermé !

                return new()
                {
                    Status = ResponseStatus.PortClosed,
                };
            }

            List<byte> inBuffer = [];
            int nullCounter = 0;
            DateTime beginTime = DateTime.Now;

            // On vide le buffer d'entrée par sécurité

            _SerialPort.DiscardInBuffer();

            try
            {
                while (timeout == 0 || DateTime.Now.Subtract(beginTime).TotalMilliseconds < timeout)
                {
                    if (_RequestCancelGetProgram)
                    {
                        return new()
                        {
                            Status = ResponseStatus.Canceled
                        };
                    }

                    while (_SerialPort.BytesToRead > 0)
                    {
                        if (_RequestCancelGetProgram)
                        {
                            return new()
                            {
                                Status = ResponseStatus.Canceled
                            };
                        }

                        beginTime = DateTime.Now;

                        var value = _SerialPort.ReadByte();

                        if (value > -1)
                        {
                            inBuffer.Add((byte)value);

                            if (value == 0)
                            {
                                nullCounter += 1;

                                if (nullCounter == 13)
                                {
                                    // On a récupéré les bytes du programme envoyé par la commande SAVE "COM:",4800 du X-07

                                    return new()
                                    {
                                        Status = ResponseStatus.Success,
                                        Value = inBuffer.ToArray()
                                    };
                                }
                            }
                            else
                            {
                                nullCounter = 0;
                            }
                        }
                    }
                }

                // Timeout !

                return new()
                {
                    Status = ResponseStatus.Timeout
                };
            }
            catch (Exception ex)
            {
                // Une exception a été levée !

                return new()
                {
                    Status = ResponseStatus.Exception,
                    Exception = ex
                };
            }
        }


        private string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(bytes[i].ToString("X2"));
            }

            return sb.ToString();
        }

        #region Classes - Enums

        public enum ResponseStatus
        {
            None,
            Success,
            Timeout,
            Exception,
            PortClosed,
            Canceled
        }

        public class Response<T>
        {
            public ResponseStatus Status { get; init; } = ResponseStatus.None;

            public Exception? Exception { get; init; }

            public T? Value { get; init; }
        }

        public class GetProgramResponse : Response<byte[]>
        {

        }

        #endregion
    }
}
