using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Project.Domain.AggregatesModel;
using MediatR;
using Project.API.Dtos;

namespace Project.API.Applications.Commands
{
    public class CreateProjectCommand : IRequest<int>
    {
        public required CreateProjectDto CreateProjectDto { get; init; }
        public required int UserId { get; init; }
        public required string UserName { get; init; }
        public required string Avatar { get; init; }

        //public Test test { get; set; }

    }

    // 用这个去做一些测试，如果postman的body中只给了Introduction和 Company字段，Avatar字段没有给值，
    // post请求会失败
    //     ```
    //     {
    //     "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    //     "title": "One or more validation errors occurred.",
    //     "status": 400,
    //     "errors": {
    //         "Avatar": [
    //             "The Avatar field is required."
    //         ]
    // },
    //     "traceId": "00-160513f998e470737b02da5899eda083-9705bd17db78b7e2-00"
    // }
    //     ```
    public class Test
    {
        public string Introduction { get; set; }
        public string Company { get; set; }

        public string Avatar { get; set; }
    }
}