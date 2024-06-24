'use client'

import React, { useEffect, useState } from 'react'

// MUI Imports
import Grid from '@mui/material/Grid'
import Card from '@mui/material/Card'
import Step from '@mui/material/Step'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import Stepper from '@mui/material/Stepper'
import TextField from '@mui/material/TextField'
import StepLabel from '@mui/material/StepLabel'
import Typography from '@mui/material/Typography'
import InputLabel from '@mui/material/InputLabel'
import CardContent from '@mui/material/CardContent'
import FormControl from '@mui/material/FormControl'
import FormHelperText from '@mui/material/FormHelperText'
import InputAdornment from '@mui/material/InputAdornment'
import IconButton from '@mui/material/IconButton'
<<<<<<< HEAD
=======
import Autocomplete from '@mui/material/Autocomplete'
import axios from 'axios'
>>>>>>> 8f5e278 (..)

// Third-party Imports
import { toast } from 'react-toastify'
import { Controller, useForm } from 'react-hook-form'
import { valibotResolver } from '@hookform/resolvers/valibot'
import { object, minLength, string } from 'valibot'

import axios from 'axios'

// Component Imports
import StepperWrapper from '@core/styles/stepper'

import StepperCustomDot from './StepperCustomDot'

// Styled Component Imports
import AppReactDatepicker from '@/libs/styles/AppReactDatepicker'

import '../../../../assets/css/myStyle.css'

// Vars
const steps = [
  {
    title: 'Personal Info',
    subtitle: 'Enter your Personal Details'
  },
  {
    title: 'Course Info',
    subtitle: 'Setup Information'
  },
  {
    title: 'Contract',
    subtitle: 'Add Social Links'
  }
]

const https = require('https')

const agent = new https.Agent({
  rejectUnauthorized: false
})

const socialSchema = object({})

const StepperLinearWithValidation = () => {
  // States
  const [activeStep, setActiveStep] = useState(0)

  const [formData, setFormData] = useState({
    FirstName: '',
    LastName: '',
    Nationality: '',
    City: '',
    Note: '',
    Birthday: '',
    Street: '',
    StreetNr: '',
    ZipCode: '',
    CourseID: '1'
  })

  const { control, reset } = useForm({})

  const [errors, setErrors] = useState({})

  const [date, setDate] = useState('')

  const handleChangeDate = date => {
    setDate(date)
  }

  // Handle form field changes
  const handleChange = e => {
    validateForm()

    const { name, value } = e.target

    setFormData({
      ...formData,
      [name]: value
    })
  }

  const validateForm = () => {
    let errors = {}
    let isValid = true

    if (!formData.FirstName.trim() && formData.FirstName == '') {
      errors.FirstName = 'This field is required'
      isValid = false
    }

    if (!formData.LastName.trim()) {
      errors.LastName = 'This field is required'
      isValid = false
    }

    if (!formData.Nationality.trim()) {
      errors.Nationality = 'This field is required'
      isValid = false
    }

    if (!formData.Note.trim()) {
      errors.Note = 'This field is required'
      isValid = false
    }

    setErrors(errors)

    return isValid
  }

  const handleSubmit = e => {
    e.preventDefault()
    setActiveStep(prevActiveStep => prevActiveStep + 1)

    if (validateForm()) {
      // Form is valid, handle submission (e.g., API call)

      if (activeStep === steps.length - 3) {
        console.log('2')
        formData.Birthday = '03-03-2022'

        console.log('Submitting form with data:', formData)
        const formData1 = new FormData()

        for (const key in formData) {
          formData1.append(key, formData[key])
        }

        axios({
          method: 'post',
          url: 'http://localhost:5008/api/Student',
          data: formData1,
          headers: { 'Content-Type': 'multipart/form-data' }
        })
          .then(function (response) {
            //handle success
            console.log(response + 'ddddd')
          })
          .catch(function (response) {
            //handle error
            console.log(response)
          })
      }
    } else {
      // Form validation failed, do something (e.g., show error messages)
      console.log('Form validation failed')
    }
  }

  const handleBack = () => {
    setActiveStep(prevActiveStep => prevActiveStep - 1)
  }

  const handleReset = () => {
    setActiveStep(0)
    personalReset({ FirstName: '', LastName: '', Nationality: '', Birthday: '', Note: '' })
    courseReset({ CourseID: '' })
  }

  const renderStepContent = activeStep => {
    switch (activeStep) {
      case 0:
        return (
          <form key={0} onSubmit={handleSubmit}>
            <Grid container spacing={5}>
              <Grid item xs={12}>
                <Typography className='font-medium' color='text.primary'>
                  {steps[0].title}
                </Typography>
                <Typography variant='body2'>{steps[0].subtitle}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='FirstName'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      fullWidth
                      {...field}
                      value={formData.FirstName}
                      onChange={handleChange}
                      label='First Name'
                      placeholder='First Name'
                    />
                  )}
                />
                {errors.FirstName && <p className='error_custom'> {errors.FirstName}</p>}
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='LastName'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      value={formData.LastName}
                      onChange={handleChange}
                      label='Last Name'
                      placeholder='Last Name'
                      value={formData.LastName}
                      onChange={handleChange}
                    />
                  )}
                />
                {errors.LastName && <span className='error_custom'>{errors.LastName}</span>}
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Birthday'
                  control={control}
                  render={({ field }) => (
                    <AppReactDatepicker
                      {...field}
                      selected={date}
                      showYearDropdown
                      showMonthDropdown
                      dateFormat='yyyy-MM-dd'
                      onChange={handleChangeDate}
                      placeholderText='MM/DD/YYYY'
                      customInput={<TextField value={date} onChange={handleChangeDate} fullWidth label='Birthday' />}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='City'
                  control={control}
                  render={({ field }) => <TextField {...field} fullWidth label='City' placeholder='City' />}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Street'
                  control={control}
                  render={({ field }) => (
                    <TextField {...field} fullWidth label='Street' placeholder='Street And House Number' />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='ZipCode'
                  control={control}
                  render={({ field }) => <TextField {...field} fullWidth label='ZipCode' placeholder='ZipCode' />}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='StreetNr'
                  control={control}
                  render={({ field }) => (
                    <TextField {...field} fullWidth label='StreetNr' placeholder='Street And House Number' />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Nationality'
                  control={control}
                  rules={{ required: true }}
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='Nationality'
                      value={formData.Nationality}
                      onChange={handleChange}
                      placeholder='Nationality'
                    />
                  )}
                />
                {errors.Nationality && <span className='error_custom'>{errors.Nationality}</span>}
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Note'
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      rows={4}
                      cols={4}
                      fullWidth
                      value={formData.Note}
                      onChange={handleChange}
                      multiline
                      label='Notes'
                    />
                  )}
                />
                {errors.Note && <span className='error_custom'>{errors.Note}</span>}
              </Grid>
              <Grid item xs={12} className='flex justify-between'>
                <Button variant='outlined' disabled color='secondary'>
                  Back
                </Button>
                <Button variant='contained' type='submit'>
                  Next
                </Button>
              </Grid>
            </Grid>
          </form>
        )
      case 1:
        return (
          <form key={1} onSubmit={handleSubmit}>
            <Grid container spacing={5}>
              <Grid item xs={12}>
                <Typography className='font-medium' color='text.primary'>
                  {steps[1].title}
                </Typography>
                <Typography variant='body2'>{steps[1].subtitle}</Typography>
              </Grid>
              <Grid item xs={12} className='flex justify-between'>
                <Button variant='outlined' onClick={handleBack} color='secondary'>
                  Back
                </Button>
                <Button variant='contained' type='submit'>
                  Next
                </Button>
              </Grid>
            </Grid>
          </form>
        )
      case 2:
        return (
          <form key={2}>
            <Grid container spacing={5}>
              <Grid item xs={12}>
                <Typography className='font-medium' color='text.primary'>
                  {steps[2].title}
                </Typography>
                <Typography variant='body2'>{steps[2].subtitle}</Typography>
              </Grid>

              <Grid item xs={12} className='flex justify-between'>
                <Button variant='outlined' onClick={handleBack} color='secondary'>
                  Back
                </Button>
                <Button variant='contained' type='submit'>
                  Submit
                </Button>
              </Grid>
          </form>
        )
    }
  }

  return (
    <Card>
      <CardContent>
        <StepperWrapper>
          <Stepper activeStep={activeStep}>
            {steps.map((label, index) => {
              const labelProps = {}

              if (index === activeStep) {
                labelProps.error = false

                if ((errors.FirstName || errors.LastName) && activeStep === 0) {
                  labelProps.error = true
                } else {
                  labelProps.error = false
                }
              }

              return (
                <Step key={index}>
                  <StepLabel {...labelProps} StepIconComponent={StepperCustomDot}>
                    <div className='step-label'>
                      <Typography className='step-number' color='text.primary'>{`0${index + 1}`}</Typography>
                      <div>
                        <Typography className='step-title' color='text.primary'>
                          {label.title}
                        </Typography>
                        <Typography className='step-subtitle' color='text.primary'>
                          {label.subtitle}
                        </Typography>
                      </div>
                    </div>
                  </StepLabel>
                </Step>
              )
            })}
          </Stepper>
        </StepperWrapper>
      </CardContent>
      <Divider />
      <CardContent>
        {activeStep === steps.length ? (
          <>
            <Typography className='mlb-2 mli-1' color='text.primary'>
              All steps are completed!
            </Typography>
            <div className='flex justify-end mt-4'>
              <Button variant='contained' onClick={handleReset}>
                Reset
              </Button>
            </div>
          </>
        ) : (
          renderStepContent(activeStep)
        )}
      </CardContent>
    </Card>
  )
}

export default StepperLinearWithValidation
