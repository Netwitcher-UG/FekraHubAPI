'use client'
import React, { useState, useEffect } from 'react'

import Grid from '@mui/material/Grid'

import ViewStudents from '@views/pages/parents/children/ViewStudents'

const ViewData = () => {
  let [studentList, setDataStudent] = useState(null)

  useEffect(() => {
    fetch('http://localhost:5008/api/Student/ByParent', {
      headers: {
        Authorization:
          'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3VsYWZfZmVrcmFodWIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjA3ODhiMzMxLTdiOGUtNDI5NC04ODJlLTU2MGM4ODRiNGY4ZiIsImp0aSI6IjA2YjI0NGJjLWVjYzQtNDU2Yy05MGJjLTI2MjhlOGI5MWQxZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlBhcmVudCIsImV4cCI6MTcxODc4NDM3OCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1NzI5OSIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTU1NTUifQ.X1Q171kW-oxPCFaODyJ7OnpNVfvDDU2kd-NMz6H0AXg'
      }
    })
      .then(response => {
        return response.json()
      })
      .then(data => {
        setDataStudent(data)
      })
  }, [])

  return (
    <div>
      <Grid container spacing={6}>
        <Grid item xs={12} md={12}>
          {studentList && <ViewStudents data={studentList} />}
        </Grid>
      </Grid>
    </div>
  )
}

export default ViewData
