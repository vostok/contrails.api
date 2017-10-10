# API

## Get traces information by trace id.

**HTTP Method**: GET

**URI**: /tracesById

GET Parameters:

Name      | Required | Type           | Default value | Description
----------|----------|----------------|---------------|------------
id        | *        | Guid           |               | TraceId
fromTs    |          | DateTime (ISO) | null          | Records selected by condition BeginTimestamp > fromTs if fromTs!=null.
fromSpan  |          | Guid           | null          | Used if fromTs defined to select records greater than specified BeginTimestamp and SpanId. 
toTs      |          | DateTime (ISO) | null          | Records selected by condition BeginTimestamp < toTs if toTs!=null.
toSpan    |          | Guid           | null          | Used if toTs defined to select records less than specified BeginTimestamp and SpanId. 
limit     |          | int            | 1000          | Maximum records count. 
ascending |          | bool           | true          | Is ascending order.

### Sample

`GET /TracesById?id=c8c6a374-acba-43d9-8f3a-f62aefb1020d&tots=2017-10-10T12%3A11%3A12.536%2B00%3A00&ascending=true`

Response:

```
{
    "TraceId": "9ff2d5548b7e4f7b8f245e9d30ad78ba",
    "Spans": [
        {
            "TraceId": "9ff2d5548b7e4f7b8f245e9d30ad78ba",
            "SpanId": "00a5e964000000000000000000000000",
            "ParentSpanId": "00a5e964000000000000000000000000",
            "BeginTimestamp": "2017-10-05T09:06:00.1520000+03:00",
            "EndTimestamp": "2017-10-05T09:06:34.5031881+03:00",
            "Annotations": {
                "OperationName": "HTTP",
                ...
            }
        },
        ...
    ]
}
```
