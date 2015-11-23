//   OData .NET Libraries ver. 5.6.3
//   Copyright (c) Microsoft Corporation
//   All rights reserved. 
//   MIT License
//   Permission is hereby granted, free of charge, to any person obtaining a copy of
//   this software and associated documentation files (the "Software"), to deal in
//   the Software without restriction, including without limitation the rights to use,
//   copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
//   Software, and to permit persons to whom the Software is furnished to do so,
//   subject to the following conditions:

//   The above copyright notice and this permission notice shall be included in all
//   copies or substantial portions of the Software.

//   THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//   FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//   COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//   IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//   CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using Microsoft.Data.OData;

    /// <summary>Use this class to customize how exceptions are handled.</summary>
    public class HandleExceptionArgs
    {
        #region Private fields

        #endregion Private fields

        #region Constructors

        /// <summary>Initalizes a new <see cref="HandleExceptionArgs"/> instance.</summary>
        /// <param name="exception">The <see cref="Exception"/> being handled.</param>
        /// <param name="responseWritten">Whether the response has already been written out.</param>
        /// <param name="contentType">The MIME type used to write the response.</param>
        /// <param name="verboseResponse">Whether a verbose response is appropriate.</param>
        internal HandleExceptionArgs(Exception exception, bool responseWritten, string contentType, bool verboseResponse)
        {
            this.Exception = WebUtil.CheckArgumentNull(exception, "exception");
            this.ResponseWritten = responseWritten;
            this.ResponseContentType = contentType;
            this.UseVerboseErrors = verboseResponse;
#pragma warning disable 618 // Disable "obsolete" warning for the InstanceAnotationCollection class and CustomAnnotations property. Used for backwards compatibilty.
            this.CustomAnnotations = new InstanceAnnotationCollection();
#pragma warning restore 618
            this.InstanceAnnotations = new Collection<ODataInstanceAnnotation>();
        }

        #endregion Constructors

        #region Public properties

        /// <summary>Gets or sets the exception that will be processed and returned in the response.</summary>
        /// <returns>The exception that will be processed and returned in the response.</returns>
        /// <remarks>This property may be null.</remarks>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>Gets the response content type.</summary>
        /// <returns>The string value that indicates the response format.</returns>
        public string ResponseContentType
        {
            get; 
            private set;
        }

        /// <summary>Gets a value indicating whether the response has been written. </summary>
        /// <returns>Boolean value that indicates whether response has been written.</returns>
        public bool ResponseWritten
        {
            get;
            private set;
        }

        /// <summary>Gets or sets a Boolean value that indicates whether verbose errors will be returned.</summary>
        /// <returns>The Boolean value that indicates whether verbose errors will be returned.</returns>
        public bool UseVerboseErrors
        {
            get;
            set;
        }

        /// <summary>Gets the status code that will be sent back in the HTTP header section of the response when an error occurs on the data service.</summary>
        /// <returns>An integer value of the HTTP response status code. </returns>
        public int ResponseStatusCode
        {
            get
            {
                return this.Exception is DataServiceException
                    ? ((DataServiceException)this.Exception).StatusCode 
                    : 500;
            }
        }

        /// <summary>
        /// Collection of custom values that will be written in the error payload as instance annotations.
        /// </summary>
        /// <remarks>
        /// These values will only be serialized in JSON.
        /// </remarks>
        [Obsolete("The CustomAnnotations property is deprecated. Use the InstanceAnnotations property instead.")]
        public InstanceAnnotationCollection CustomAnnotations { get; private set; }

        /// <summary>
        /// Collection of custom values that will be written in the error payload as instance annotations.
        /// </summary>
        /// <remarks>
        /// These values will only be serialized in JSON and if any instance annotation is added to the InstanceAnnotations collection,
        /// the CustomAnnotations property is ignored during serialization, 
        /// </remarks>
        public ICollection<ODataInstanceAnnotation> InstanceAnnotations { get; private set; }

        #endregion Public properties

        #region Internal properties

        /// <summary>The value for the 'Allow' response header.</summary>
        internal string ResponseAllowHeader
        {
            get
            {
                return this.Exception is DataServiceException
                    ? ((DataServiceException)this.Exception).ResponseAllowHeader
                    : null;
            }
        }

#if DEBUG
        /// <summary>
        /// This property should get set to true when ProcessException is called on DataService.
        /// Then, we can assert that it is true when we start serializing errors.
        /// Thus we know that ProcessException is being called for every error scenario.
        /// </summary>
        internal bool ProcessExceptionWasCalled { get; set; }
#endif
        #endregion Internal properties

        /// <summary>Creates an ODataError instance describing the error.</summary>
        /// <returns>A new ODataError instance describing the error.</returns>
        internal ODataError CreateODataError()
        {
            ODataError error = new ODataError();
            DataServiceException dataException = this.Exception as DataServiceException;
            if (dataException == null)
            {
                error.ErrorCode = String.Empty;
                error.Message = Strings.DataServiceException_GeneralError;
                error.MessageLanguage = CultureInfo.CurrentCulture.Name;

                if (this.UseVerboseErrors)
                {
                    error.InnerError = new ODataInnerError(this.Exception);
                }
            }
            else
            {
                error.ErrorCode = dataException.ErrorCode ?? String.Empty;
                error.Message = dataException.Message ?? String.Empty;
                error.MessageLanguage = dataException.MessageLanguage ?? CultureInfo.CurrentCulture.Name;

                if (this.UseVerboseErrors && dataException.InnerException != null)
                {
                    error.InnerError = new ODataInnerError(dataException.InnerException);
                }
            }

#pragma warning disable 618 // Disable "obsolete" warning for the CustomAnnotations property. Used for backwards compatibilty.
            error.SetAnnotation(this.CustomAnnotations);
#pragma warning restore 618
            error.InstanceAnnotations = this.InstanceAnnotations;

            return error;
        }
    }
}