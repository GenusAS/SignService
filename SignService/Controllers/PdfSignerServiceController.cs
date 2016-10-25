﻿/*
This file is part of the Genus PdfSignerService (R) project.
Copyright (c) 2016 Genus AS, Norway
Author(s): Sverre Hårstadstrand

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.
*/
namespace SignService.Controllers
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using SignService.Models;

    /// <summary>
    /// Receives a PDF-document to sign in the body of the POST-request. Signs 
    /// the document and returns it in the body of the response.
    /// </summary>
    public class PdfSignerServiceController : ApiController
    {
        /// <summary>
        /// The POST method handler.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        public HttpResponseMessage Post()
        {
            var l_task = this.Request.Content.ReadAsStreamAsync();
            l_task.Wait();
            var l_requestStream = l_task.Result;

            var l_filename = System.IO.Path.GetTempFileName();

            PdfDocumentSigner.SignDocumentStream(l_requestStream, l_filename);

            var l_response = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                l_response.Content = this.TemporaryFile(l_filename);
                l_response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                l_response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "SignedDocument.pdf"
                };
            }
            catch (Exception l_ex)
            {
                // log your exception details here
                l_response =
                    new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(l_ex.Message)
                    };
            }

            return l_response;

        }

        /// <summary>
        /// Reads the contents of the temporary signed file into a HttpContent instance, and deletes the file.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The <see cref="HttpContent"/>.
        /// </returns>
        private HttpContent TemporaryFile(string fileName)
        {
            var l_bytes = File.ReadAllBytes(fileName);
            File.Delete(fileName);
            return new StreamContent(new MemoryStream(l_bytes));
        }
    }
}
